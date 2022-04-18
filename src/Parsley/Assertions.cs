using System.Text;

namespace Parsley;

public static class Assertions
{
    public static Reply<T> FailsToParse<T>(this Parser<T> parse, string input, string expectedUnparsedInput, string expectedMessage)
    {
        var text = new Text(input);
        var reply = parse(ref text).Fails(ref text, expectedMessage);
        
        if (expectedUnparsedInput == "")
            text.AtEndOfInput();
        else
            text.LeavingUnparsedInput(expectedUnparsedInput);

        return reply;
    }

    public static Reply<T> PartiallyParses<T>(this Parser<T> parse, string input, string expectedUnparsedInput)
    {
        var text = new Text(input);
        var reply = parse(ref text).Succeeds(ref text);

        if (expectedUnparsedInput == "")
            throw new ArgumentException($"{nameof(expectedUnparsedInput)} must be nonempty when calling {nameof(PartiallyParses)}.");

        text.LeavingUnparsedInput(expectedUnparsedInput);

        return reply;
    }

    public static Reply<T> Parses<T>(this Parser<T> parse, string input)
    {
        var text = new Text(input);
        var reply = parse(ref text).Succeeds(ref text);

        text.AtEndOfInput();

        return reply;
    }

    static Reply<T> Fails<T>(this Reply<T> reply, ref Text text, string expectedMessage)
    {
        if (reply.Success)
            throw new AssertionException("parser failure", "parser completed successfully");

        var position = text.Position;

        var actual = position + ": " + reply.Expectation + " expected";
            
        if (actual != expectedMessage)
            throw new AssertionException($"message at {expectedMessage}", $"message at {actual}");

        return reply;
    }

    static Reply<T> Succeeds<T>(this Reply<T> reply, ref Text text)
    {
        if (!reply.Success)
        {
            var peek = text.Peek(20).ToString();

            var offendingCharacter = peek[0];
            var displayFriendlyTrailingCharacters = new string(peek.Skip(1).TakeWhile(x => !char.IsControl(x)).ToArray());

            var message = new StringBuilder();
            message.AppendLine(text.Position + ": " + reply.Expectation);
            message.AppendLine();
            message.AppendLine($"\t{offendingCharacter}{displayFriendlyTrailingCharacters}");
            message.AppendLine("\t^");

            throw new AssertionException(message.ToString(), "parser success", "parser failure");
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
