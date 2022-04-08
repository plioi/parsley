namespace Parsley.Primitives;

class AttemptParser<T> : IParser<T>
{
    readonly IParser<T> parse;

    public AttemptParser(IParser<T> parse)
    {
        this.parse = parse;
    }

    public Reply<T> Parse(Text input)
    {
        var start = input.Position;
        var reply = parse.Parse(input);
        var newPosition = reply.UnparsedInput.Position;

        if (reply.Success || start == newPosition)
            return reply;

        return new Error<T>(input, ErrorMessage.Backtrack(newPosition, reply.ErrorMessages));
    }
}
