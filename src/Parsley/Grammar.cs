namespace Parsley;

public static partial class Grammar
{
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

    /// <summary>
    /// Optional(p) is equivalent to p whenever p succeeds or when p fails after consuming input.
    /// If p fails without consuming input, Optional(p) succeeds.
    /// </summary>
    public static Parser<T> Optional<T>(Parser<T> parser)
    {
        var nothing = default(T).SucceedWithThisValue();
        return Choice(parser, nothing);
    }

    static IEnumerable<T> List<T>(T first, IEnumerable<T> rest)
    {
        yield return first;

        foreach (var item in rest)
            yield return item;
    }

    static Parser<IEnumerable<T>> Zero<T>()
    {
        return Enumerable.Empty<T>().SucceedWithThisValue();
    }
}
