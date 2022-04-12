namespace Parsley.Primitives;

class ChoiceParser<T> : Parser<T>
{
    readonly Parser<T>[] parsers;

    public ChoiceParser(Parser<T>[] parsers)
    {
        this.parsers = parsers;
    }

    public Reply<T> Parse(Text input)
    {
        var start = input.Position;
        var reply = parsers[0](input);
        var newPosition = reply.UnparsedInput.Position;

        var errors = ErrorMessageList.Empty;
        var i = 1;
        while (!reply.Success && (start == newPosition) && i < parsers.Length)
        {
            errors = errors.Merge(reply.ErrorMessages);
            reply = parsers[i](input);
            newPosition = reply.UnparsedInput.Position;
            i++;
        }
        if (start == newPosition)
        {
            errors = errors.Merge(reply.ErrorMessages);
            if (reply.Success)
                reply = new Parsed<T>(reply.Value, reply.UnparsedInput, errors);
            else
                reply = new Error<T>(reply.UnparsedInput, errors);
        }

        return reply;
    }
}
