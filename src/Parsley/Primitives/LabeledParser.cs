namespace Parsley.Primitives;

class LabeledParser<T> : Parser<T>
{
    readonly Parser<T> parse;
    readonly ErrorMessageList errors;

    public LabeledParser(Parser<T> parse, string expectation)
    {
        this.parse = parse;
        errors = ErrorMessageList.Empty.With(ErrorMessage.Expected(expectation));
    }

    public Reply<T> Parse(Text input)
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
    }
}
