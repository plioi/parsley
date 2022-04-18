using System.Diagnostics.CodeAnalysis;

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
        return (ref Text input, [NotNullWhen(true)] out IEnumerable<T>? values, [NotNullWhen(false)] out string? expectation) =>
        {
            var oldPosition = input.Position;
            var succeeded = item(ref input, out var itemValue, out var itemExpectation);
            var newPosition = input.Position;

            var list = new List<T>();

            while (succeeded)
            {
                if (oldPosition == newPosition)
                    throw new Exception($"Parser encountered a potential infinite loop at position {newPosition}.");

                list.Add(itemValue!);
                oldPosition = newPosition;
                succeeded = item(ref input, out itemValue, out itemExpectation);
                newPosition = input.Position;
            }

            //The item parser finally failed.

            if (oldPosition != newPosition)
            {
                expectation = itemExpectation!;
                values = null;
                return false;
            }

            expectation = null;
            values = list;
            return true;
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
        return (ref Text input, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var span = input.TakeWhile(test);

            if (span.Length > 0)
            {
                input.Advance(span.Length);

                expectation = null;
                value = span.ToString();
                return true;
            }

            expectation = null;
            value = "";
            return true;
        };
    }

    public static Parser<string> OneOrMore(Predicate<char> test, string name)
    {
        return (ref Text input, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var span = input.TakeWhile(test);

            if (span.Length > 0)
            {
                input.Advance(span.Length);

                expectation = null;
                value = span.ToString();
                return true;
            }

            expectation = name;
            value = null;
            return false;
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

