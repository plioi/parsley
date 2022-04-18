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
        return (ref Text input) =>
        {
            var oldPosition = input.Position;
            var reply = item(ref input);
            var newPosition = input.Position;

            var list = new List<T>();

            while (reply.Success)
            {
                if (oldPosition == newPosition)
                    throw new Exception($"Parser encountered a potential infinite loop at position {newPosition}.");

                list.Add(reply.Value);
                oldPosition = newPosition;
                reply = item(ref input);
                newPosition = input.Position;
            }

            //The item parser finally failed.

            if (oldPosition != newPosition)
                return new Error<IEnumerable<T>>(reply.Expectation);

            return new Parsed<IEnumerable<T>>(list);
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

    public static Parser<string> ZeroOrMore(Predicate<char> test)
    {
        return (ref Text input) =>
        {
            var value = input.TakeWhile(test);

            if (value.Length > 0)
            {
                input.Advance(value.Length);

                return new Parsed<string>(value.ToString());
            }

            return new Parsed<string>("");
        };
    }

    public static Parser<string> OneOrMore(Predicate<char> test, string name)
    {
        return (ref Text input) =>
        {
            var value = input.TakeWhile(test);

            if (value.Length > 0)
            {
                input.Advance(value.Length);

                return new Parsed<string>(value.ToString());
            }

            return new Error<string>(name);
        };
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

