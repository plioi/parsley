namespace Parsley;

public static class ParsingAssertions
{
    public static void ShouldSucceed(this MatchResult actual, string expected)
    {
        if (!actual.Success)
            throw new AssertionException("successful match", "match failed");

        if (actual.Value != expected)
            throw new AssertionException(expected, actual.Value);
    }

    public static void ShouldFail(this MatchResult actual)
    {
        if (actual.Success)
            throw new AssertionException("match failure", "successful match");

        const string expected = "";

        if (actual.Value != expected)
            throw new AssertionException(expected, actual.Value);
    }

    public static Reply<T> FailsToParse<T>(this IParser<T> parser, string input)
    {
        var reply = parser.Parse(new Text(input));
            
        if (reply.Success)
            throw new AssertionException("parser failure", "parser completed successfully");

        return reply;
    }

    public static Reply<T> WithMessage<T>(this Reply<T> reply, string expectedMessage)
    {
        var position = reply.UnparsedInput.Position;
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

    public static Reply<T> PartiallyParses<T>(this IParser<T> parser, string input)
    {
        return parser.Parse(new Text(input)).Succeeds();
    }

    public static Reply<T> Parses<T>(this IParser<T> parser, string input)
    {
        return parser.Parse(new Text(input)).Succeeds().AtEndOfInput();
    }

    static Reply<T> Succeeds<T>(this Reply<T> reply)
    {
        if (!reply.Success)
        {
            var message = "Position: " + reply.UnparsedInput.Position
                                       + Environment.NewLine
                                       + "Error Message: " + reply.ErrorMessages;
            throw new AssertionException(message, "parser success", "parser failed");
        }

        return reply;
    }

    public static Reply<T> LeavingUnparsedInput<T>(this Reply<T> reply, string expectedUnparsedInput)
    {
        var actualUnparsedInput = reply.UnparsedInput.ToString();

        if (actualUnparsedInput != expectedUnparsedInput)
            throw new AssertionException("Parse resulted in unexpected remaining unparsed input.",
                expectedUnparsedInput,
                actualUnparsedInput);

        return reply;
    }

    public static Reply<T> AtEndOfInput<T>(this Reply<T> reply)
    {
        if (!reply.UnparsedInput.EndOfInput)
            throw new AssertionException("end of input", reply.UnparsedInput);

        return reply.LeavingUnparsedInput("");
    }

    public static Reply<T> WithValue<T>(this Reply<T> reply, T expected)
    {
        if (!Equals(expected, reply.Value))
            throw new AssertionException($"parsed value: {expected}", $"parsed value: {reply.Value}");

        return reply;
    }

    public static Reply<T> WithValue<T>(this Reply<T> reply, Action<T> assertParsedValue)
    {
        assertParsedValue(reply.Value);

        return reply;
    }
}
