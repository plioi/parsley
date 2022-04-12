namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// The parser Attempt(p) behaves like parser p, except that it pretends
    /// that it hasn't consumed any input when an error occurs. This combinator
    /// is used whenever arbitrary look ahead is needed.
    /// </summary>
    public static Parser<T> Attempt<T>(Parser<T> parse)
    {
        return input =>
        {
            var snapshot = input.Snapshot();
            var start = input.Position;
            var reply = parse(input);
            var newPosition = reply.Position;

            if (reply.Success || start == newPosition)
                return reply;

            input.Restore(snapshot);
            return new Error<T>(input, input.Position, input.EndOfInput, ErrorMessage.Backtrack(newPosition, reply.ErrorMessages));
        };
    }
}
