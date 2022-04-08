namespace Parsley.Primitives;

class ZeroOrMoreParser<T> : IParser<IEnumerable<T>>
{
    readonly IParser<T> item;

    public ZeroOrMoreParser(IParser<T> item)
    {
        this.item = item;
    }

    public Reply<IEnumerable<T>> Parse(Text input)
    {
        var oldPosition = input.Position;
        var reply = item.Parse(input);
        var newPosition = reply.UnparsedInput.Position;

        var list = new List<T>();

        while (reply.Success)
        {
            if (oldPosition == newPosition)
                throw new Exception($"Parser encountered a potential infinite loop at position {newPosition}.");

            list.Add(reply.Value);
            oldPosition = newPosition;
            reply = item.Parse(reply.UnparsedInput);
            newPosition = reply.UnparsedInput.Position;
        }

        //The item parser finally failed.

        if (oldPosition != newPosition)
            return new Error<IEnumerable<T>>(reply.UnparsedInput, reply.ErrorMessages);

        return new Parsed<IEnumerable<T>>(list, reply.UnparsedInput, reply.ErrorMessages);
    }
}
