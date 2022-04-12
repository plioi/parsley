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
            var newPosition = reply.Position;

            var list = new List<T>();

            while (reply.Success)
            {
                if (oldPosition == newPosition)
                    throw new Exception($"Parser encountered a potential infinite loop at position {newPosition}.");

                list.Add(reply.Value);
                oldPosition = newPosition;
                reply = item(reply.UnparsedInput);
                newPosition = reply.Position;
            }

            //The item parser finally failed.

            if (oldPosition != newPosition)
            {
                Text unparsedInput = reply.UnparsedInput;

                return new Error<IEnumerable<T>>(unparsedInput, unparsedInput.Position, unparsedInput.EndOfInput, reply.ErrorMessages);
            }

            Text unparsedInput1 = reply.UnparsedInput;

            return new Parsed<IEnumerable<T>>(list, unparsedInput1, unparsedInput1.Position, unparsedInput1.EndOfInput, reply.ErrorMessages);
        };
    }

    /// <summary>
    /// OneOrMore(p) behaves like ZeroOrMore(p), except that p must succeed at least one time.
    /// </summary>
    public static Parser<IEnumerable<T>> OneOrMore<T>(Parser<T> item)
    {
        return from first in item
            from rest in ZeroOrMore(item)
            select List(first, rest);
    }

    /// <summary>
    /// ZeroOrMore(p, s) parses zero or more occurrences of p separated by occurrences of s,
    /// returning the list of values returned by successful applications of p.
    /// </summary>
    public static Parser<IEnumerable<T>> ZeroOrMore<T, S>(Parser<T> item, Parser<S> separator)
    {
        return Choice(OneOrMore(item, separator), Zero<T>());
    }

    /// <summary>
    /// OneOrMore(p, s) behaves like ZeroOrMore(p, s), except that p must succeed at least one time.
    /// </summary>
    public static Parser<IEnumerable<T>> OneOrMore<T, S>(Parser<T> item, Parser<S> separator)
    {
        return from first in item
            from rest in ZeroOrMore(from sep in separator
                from next in item
                select next)
            select List(first, rest);
    }

    static Parser<IEnumerable<T>> Zero<T>()
    {
        return Enumerable.Empty<T>().SucceedWithThisValue();
    }

    static IEnumerable<T> List<T>(T first, IEnumerable<T> rest)
    {
        yield return first;

        foreach (var item in rest)
            yield return item;
    }
}

