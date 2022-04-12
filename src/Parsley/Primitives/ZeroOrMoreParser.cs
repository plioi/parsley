namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// ZeroOrMore(p) repeatedly applies an parser p until it fails, returning
    /// the list of values returned by successful applications of p.  At the
    /// end of the sequence, p must fail without consuming input, otherwise the
    /// sequence will fail with the error reported by p.
    /// </summary>
    public static Parser<IEnumerable<T>> ZeroOrMore<T>(Parser<T> item)
    {
        return input =>
        {
            var oldPosition = input.Position;
            var reply = item(input);
            var newPosition = reply.UnparsedInput.Position;

            var list = new List<T>();

            while (reply.Success)
            {
                if (oldPosition == newPosition)
                    throw new Exception($"Parser encountered a potential infinite loop at position {newPosition}.");

                list.Add(reply.Value);
                oldPosition = newPosition;
                reply = item(reply.UnparsedInput);
                newPosition = reply.UnparsedInput.Position;
            }

            //The item parser finally failed.

            if (oldPosition != newPosition)
                return new Error<IEnumerable<T>>(reply.UnparsedInput, reply.ErrorMessages);

            return new Parsed<IEnumerable<T>>(list, reply.UnparsedInput, reply.ErrorMessages);
        };
    }
}

