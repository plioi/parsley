namespace Parsley.Primitives;

class ChoiceParser<T> : IParser<T>
{
    readonly IParser<T>[] parsers;

    public ChoiceParser(IParser<T>[] parsers)
    {
        this.parsers = parsers;
    }

    public Reply<T> Parse(Input input)
    {
        var start = input.Position;
        var reply = parsers[0].Parse(input);
        var newPosition = reply.UnparsedTokens.Position;

        var errors = ErrorMessageList.Empty;
        var i = 1;
        while (!reply.Success && (start == newPosition) && i < parsers.Length)
        {
            errors = errors.Merge(reply.ErrorMessages);
            reply = parsers[i].Parse(input);
            newPosition = reply.UnparsedTokens.Position;
            i++;
        }
        if (start == newPosition)
        {
            errors = errors.Merge(reply.ErrorMessages);
            if (reply.Success)
                reply = new Parsed<T>(reply.Value, reply.UnparsedTokens, errors);
            else
                reply = new Error<T>(reply.UnparsedTokens, errors);
        }

        return reply;
    }
}
