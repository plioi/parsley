namespace Parsley.Primitives;

class LabeledParser<T> : IParser<T>
{
    readonly IParser<T> parser;
    readonly ErrorMessageList errors;

    public LabeledParser(IParser<T> parser, string expectation)
    {
        this.parser = parser;
        errors = ErrorMessageList.Empty.With(ErrorMessage.Expected(expectation));
    }

    public Reply<T> Parse(Input input)
    {
        var start = input.Position;
        var reply = parser.Parse(input);
        var newPosition = reply.UnparsedTokens.Position;
        if (start == newPosition)
        {
            if (reply.Success)
                reply = new Parsed<T>(reply.Value, reply.UnparsedTokens, errors);
            else
                reply = new Error<T>(reply.UnparsedTokens, errors);
        }
        return reply;
    }
}
