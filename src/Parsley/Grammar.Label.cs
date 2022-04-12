namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// When parser p consumes any input, Label(p, e) is the same as p.
    /// When parser p does not consume any input, Label(p, e) is the same
    /// as p, except any messages are replaced with expectation e.
    /// </summary>
    public static Parser<T> Label<T>(Parser<T> parse, string expectation)
    {
        var errors = ErrorMessageList.Empty.With(ErrorMessage.Expected(expectation));

        return input =>
        {
            var start = input.Position;
            var reply = parse(input);
            var newPosition = reply.UnparsedInput.Position;

            if (start == newPosition)
            {
                if (reply.Success)
                    reply = new Parsed<T>(reply.Value, reply.UnparsedInput, errors);
                else
                    reply = new Error<T>(reply.UnparsedInput, errors);
            }

            return reply;
        };
    }
}
