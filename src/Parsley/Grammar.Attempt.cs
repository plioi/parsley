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
        return (ref Text input) =>
        {
            var snapshot = input;
            var start = input.Position;
            var reply = parse(ref input);
            var newPosition = input.Position;

            if (reply.Success || start == newPosition)
                return reply;

            input = snapshot;

            return new Error<T>(reply.Expectation + " at " + newPosition);
        };
    }
}
