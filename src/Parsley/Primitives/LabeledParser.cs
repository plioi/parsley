namespace Parsley.Primitives;

class LabeledParser<T> : Parser<T>
{
    readonly Parser<T> parser;
    readonly ErrorMessageList errors;

    public LabeledParser(Parser<T> parser, string expectation)
    {
        this.parser = parser;
        errors = ErrorMessageList.Empty.With(ErrorMessage.Expected(expectation));
    }

    public Reply<T> Parse(Text input)
    {
        var start = input.Position;
        var reply = parser.Parse(input);
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
