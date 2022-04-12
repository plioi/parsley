namespace Parsley.Primitives;

class AttemptParser<T> : Parser<T>
{
    readonly Parser<T> parse;

    public AttemptParser(Parser<T> parse)
    {
        this.parse = parse;
    }

    public Reply<T> Parse(Text input)
    {
        var start = input.Position;
        var reply = parse(input);
        var newPosition = reply.UnparsedInput.Position;

        if (reply.Success || start == newPosition)
            return reply;

        return new Error<T>(input, ErrorMessage.Backtrack(newPosition, reply.ErrorMessages));
    }
}
