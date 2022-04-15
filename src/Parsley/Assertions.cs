namespace Parsley;

public static class Assertions
{
    public static Reply<T> FailsToParse<T>(this Parser<T> parse, string input, string expectedUnparsedInput, string expectedMessage)
    {
        var text = new Text(input);
        var reply = parse(ref text).Fails(expectedMessage);
        
        if (expectedUnparsedInput == "")
            text.AtEndOfInput();
        else
            text.LeavingUnparsedInput(expectedUnparsedInput);

        return reply;
    }

    public static Reply<T> PartiallyParses<T>(this Parser<T> parse, string input, string expectedUnparsedInput, string? expectedMessage = null)
    {
        var text = new Text(input);
        var reply = parse(ref text).Succeeds(expectedMessage);

        text.LeavingUnparsedInput(expectedUnparsedInput);

        return reply;
    }

    public static Reply<T> Parses<T>(this Parser<T> parse, string input, string? expectedMessage = null)
    {
        var text = new Text(input);
        var reply = parse(ref text).Succeeds(expectedMessage);

        text.AtEndOfInput();

        return reply;
    }

    static Reply<T> Fails<T>(this Reply<T> reply, string expectedMessage)
    {
        if (reply.Success)
            throw new AssertionException("parser failure", "parser completed successfully");

        reply.WithMessage(expectedMessage);

        return reply;
    }

    static Reply<T> Succeeds<T>(this Reply<T> reply, string? expectedMessage = null)
    {
        if (!reply.Success)
        {
            var message = "Position: " + reply.Position
                                       + Environment.NewLine
                                       + "Error Message: " + reply.ErrorMessages;

            throw new AssertionException(message, "parser success", "parser failed");
        }

        reply.WithMessage(expectedMessage);

        return reply;
    }

    static void WithMessage<T>(this Reply<T> reply, string? expectedMessage)
    {
        if (expectedMessage == null)
        {
            if (reply.ErrorMessages != ErrorMessageList.Empty)
                throw new AssertionException("no error message", reply.ErrorMessages);
        }
        else
        {
            var position = reply.Position;
            var actual = position + ": " + reply.ErrorMessages;
            
            if (actual != expectedMessage)
                throw new AssertionException($"message at {expectedMessage}", $"message at {actual}");
        }
    }

    static void LeavingUnparsedInput(this Text text, string expectedUnparsedInput)
    {
        var actualUnparsedInput = text.ToString();

        if (actualUnparsedInput != expectedUnparsedInput)
            throw new AssertionException("Parse resulted in unexpected remaining unparsed input.",
                expectedUnparsedInput,
                actualUnparsedInput);
    }

    static void AtEndOfInput(this Text text)
    {
        if (!text.EndOfInput)
            throw new AssertionException("end of input", text.ToString());

        text.LeavingUnparsedInput("");
    }
}
