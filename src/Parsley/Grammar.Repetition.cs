using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// Repeat(p, n) expects parser p to succeed exactly n times in a row,
    /// returning the list of values returned by the successful applications of p.
    /// If parser p fails during any of the n attempts, the sequence will fail
    /// with the error reported by p.
    /// </summary>
    public static Parser<TItem, TValue[]> Repeat<TItem, TValue>(Parser<TItem, TValue> item, int count)
    {
        if (count <= 1)
            throw new ArgumentException(
                $"{nameof(Repeat)} requires the given count to be > 1.", nameof(count));

        return (ReadOnlySpan<TItem> input, ref int index, [NotNullWhen(true)] out TValue[]? values, [NotNullWhen(false)] out string? expectation) =>
        {
            var items = new TValue[count];

            for (int i = 0; i < count; i++)
            {
                if (item(input, ref index, out var itemValue, out var itemExpectation))
                {
                    items[i] = itemValue;
                }
                else
                {
                    expectation = itemExpectation;
                    values = null;
                    return false;
                }
            }

            expectation = null;
            values = items;
            return true;
        };
    }

    /// <summary>
    /// ZeroOrMore(p) repeatedly applies an parser p until it fails, returning
    /// the list of values returned by successful applications of p.  At the
    /// end of the sequence, p must fail without consuming input, otherwise the
    /// sequence will fail with the error reported by p.
    /// </summary>
    public static Parser<TItem, IEnumerable<TValue>> ZeroOrMore<TItem, TValue>(Parser<TItem, TValue> item)
    {
        return (ReadOnlySpan<TItem> input, ref int index, [NotNullWhen(true)] out IEnumerable<TValue>? values, [NotNullWhen(false)] out string? expectation) =>
        {
            var accumulator = new List<TValue>();

            return Repeat(accumulator, item, input, ref index, out values, out expectation);
        };
    }

    /// <summary>
    /// OneOrMore(p) behaves like ZeroOrMore(p), except that p must succeed at least one time.
    /// </summary>
    public static Parser<TItem, IEnumerable<TValue>> OneOrMore<TItem, TValue>(Parser<TItem, TValue> item)
    {
        return (ReadOnlySpan<TItem> input, ref int index, [NotNullWhen(true)] out IEnumerable<TValue>? values, [NotNullWhen(false)] out string? expectation) =>
        {
            var accumulator = new List<TValue>();

            if (item(input, ref index, out var value, out var itemExpectation))
            {
                accumulator.Add(value);
            }
            else
            {
                //The required first item failed.
                expectation = itemExpectation;
                values = null;
                return false;
            }

            return Repeat(accumulator, item, input, ref index, out values, out expectation);
        };
    }

    /// <summary>
    /// ZeroOrMore(p, s) parses zero or more occurrences of p separated by occurrences of s,
    /// returning the list of values returned by successful applications of p.
    /// </summary>
    public static Parser<TItem, IEnumerable<TValue>> ZeroOrMore<TItem, TValue, S>(Parser<TItem, TValue> item, Parser<TItem, S> separator)
    {
        return (ReadOnlySpan<TItem> input, ref int index, [NotNullWhen(true)] out IEnumerable<TValue>? values, [NotNullWhen(false)] out string? expectation) =>
        {
            var accumulator = new List<TValue>();
            var originalIndex = index;

            if (item(input, ref index, out var value, out var itemExpectation))
            {
                accumulator.Add(value);
            }
            else
            {
                //The optional first item failed, but if any input was consumed
                //then we must treat that as a failure rather than 'zero' items.

                if (originalIndex != index)
                {
                    expectation = itemExpectation;
                    values = null;
                    return false;
                }

                expectation = null;
                values = accumulator;
                return true;
            }

            var separatorAndNextItem =
                from _ in separator
                from next in item
                select next;

            return Repeat(accumulator, separatorAndNextItem, input, ref index, out values, out expectation);
        };
    }

    /// <summary>
    /// OneOrMore(p, s) behaves like ZeroOrMore(p, s), except that p must succeed at least one time.
    /// </summary>
    public static Parser<TItem, IEnumerable<TValue>> OneOrMore<TItem, TValue, S>(Parser<TItem, TValue> item, Parser<TItem, S> separator)
    {
        return (ReadOnlySpan<TItem> input, ref int index, [NotNullWhen(true)] out IEnumerable<TValue>? values, [NotNullWhen(false)] out string? expectation) =>
        {
            var accumulator = new List<TValue>();

            if (item(input, ref index, out var value, out var itemExpectation))
            {
                accumulator.Add(value);
            }
            else
            {
                //The required first item failed.
                expectation = itemExpectation;
                values = null;
                return false;
            }

            var separatorAndNextItem =
                from _ in separator
                from next in item
                select next;

            return Repeat(accumulator, separatorAndNextItem, input, ref index, out values, out expectation);
        };
    }

    static bool Repeat<TItem, TValue>(List<TValue> accumulator,
                                      Parser<TItem, TValue> item,
                                      ReadOnlySpan<TItem> input,
                                      ref int index,
                                      [NotNullWhen(true)] out IEnumerable<TValue>? values,
                                      [NotNullWhen(false)] out string? expectation)
    {
        var oldIndex = index;
        string? itemExpectation;

        while (item(input, ref index, out var itemValue, out itemExpectation))
        {
            if (oldIndex == index)
                throw new Exception($"Parser encountered a potential infinite loop at index {index}.");

            accumulator.Add(itemValue);

            oldIndex = index;
        }

        //The item parser finally failed.

        if (oldIndex != index)
        {
            expectation = itemExpectation;
            values = null;
            return false;
        }

        expectation = null;
        values = accumulator;
        return true;
    }

    public static Parser<char, string> ZeroOrMore(Func<char, bool> test)
    {
        return (ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var span = input.TakeWhile(index, test);

            if (span.Length > 0)
            {
                index += span.Length;

                expectation = null;
                value = span.ToString();
                return true;
            }

            expectation = null;
            value = "";
            return true;
        };
    }

    public static Parser<char, string> OneOrMore(Func<char, bool> test, string name)
    {
        return (ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var span = input.TakeWhile(index, test);

            if (span.Length > 0)
            {
                index += span.Length;

                expectation = null;
                value = span.ToString();
                return true;
            }

            expectation = name;
            value = null;
            return false;
        };
    }
}

