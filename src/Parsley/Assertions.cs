namespace Parsley;

public static class Assertions
{
    public static Reply<T> FailsToParse<T>(this Parser<T> parse, string input, string expectedUnparsedInput, string expectedMessage)
    {
        var text = new Text(input);
        var reply = parse(ref text);
            
        if (reply.Success)
            throw new AssertionException("parser failure", "parser completed successfully");

        text.LeavingUnparsedInput(expectedUnparsedInput);
        
        if (expectedUnparsedInput == "")
            text.AtEndOfInput();

        reply.WithMessage(expectedMessage);
        
        return reply;
    }

    public static Reply<T> WithMessage<T>(this Reply<T> reply, string expectedMessage)
    {
        var position = reply.Position;
        var actual = position + ": " + reply.ErrorMessages;
            
        if (actual != expectedMessage)
            throw new AssertionException($"message at {expectedMessage}", $"message at {actual}");

        return reply;
    }

    public static Reply<T> WithNoMessage<T>(this Reply<T> reply)
    {
        if (reply.ErrorMessages != ErrorMessageList.Empty)
            throw new AssertionException("no error message", reply.ErrorMessages);

        return reply;
    }

    public static Reply<T> PartiallyParses<T>(this Parser<T> parse, string input, string expectedUnparsedInput, string? expectedMessage = null)
    {
        var text = new Text(input);

        var reply = parse(ref text).Succeeds();

        text.LeavingUnparsedInput(expectedUnparsedInput);

        if (expectedMessage == null)
            reply.WithNoMessage();
        else
            reply.WithMessage(expectedMessage);

        return reply;
    }

    public static Reply<T> Parses<T>(this Parser<T> parse, string input, string? expectedMessage = null)
    {
        var text = new Text(input);
        var reply = parse(ref text).Succeeds();

        text.AtEndOfInput();

        if (expectedMessage == null)
            reply.WithNoMessage();
        else
            reply.WithMessage(expectedMessage);

        return reply;
    }

    static Reply<T> Succeeds<T>(this Reply<T> reply)
    {
        if (!reply.Success)
        {
            var message = "Position: " + reply.Position
                                       + Environment.NewLine
                                       + "Error Message: " + reply.ErrorMessages;
            throw new AssertionException(message, "parser success", "parser failed");
        }

        return reply;
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
