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

        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var items = new TValue[count];

            for (int i = 0; i < count; i++)
            {
                var itemValue = item(input, ref index, out var itemSucceeded, out var itemExpectation);

                if (itemSucceeded)
                {
                    items[i] = itemValue!;
                }
                else
                {
                    expectation = itemExpectation;
                    succeeded = false;
                    return null;
                }
            }

            expectation = null;
            succeeded = true;
            return items;
        };
    }

    /// <summary>
    /// ZeroOrMore(p) repeatedly applies an parser p until it fails, returning
    /// the list of values returned by successful applications of p.  At the
    /// end of the sequence, p must fail without consuming input, otherwise the
    /// sequence will fail with the error reported by p.
    /// </summary>
    public static Parser<TItem, IReadOnlyList<TValue>> ZeroOrMore<TItem, TValue>(Parser<TItem, TValue> item)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var accumulator = new List<TValue>();

            return Repeat(accumulator, item, input, ref index, out succeeded, out expectation);
        };
    }

    /// <summary>
    /// OneOrMore(p) behaves like ZeroOrMore(p), except that p must succeed at least one time.
    /// </summary>
    public static Parser<TItem, IReadOnlyList<TValue>> OneOrMore<TItem, TValue>(Parser<TItem, TValue> item)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var accumulator = new List<TValue>();

            var value = item(input, ref index, out var itemSucceeded, out var itemExpectation);

            if (itemSucceeded)
            {
                accumulator.Add(value!);
            }
            else
            {
                //The required first item failed.
                expectation = itemExpectation;
                succeeded = false;
                return null;
            }

            return Repeat(accumulator, item, input, ref index, out succeeded, out expectation);
        };
    }

    /// <summary>
    /// ZeroOrMore(p, s) parses zero or more occurrences of p separated by occurrences of s,
    /// returning the list of values returned by successful applications of p.
    /// </summary>
    public static Parser<TItem, IReadOnlyList<TValue>> ZeroOrMore<TItem, TValue, S>(Parser<TItem, TValue> item, Parser<TItem, S> separator)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var accumulator = new List<TValue>();
            var originalIndex = index;

            var value = item(input, ref index, out var itemSucceeded, out var itemExpectation);

            if (itemSucceeded)
            {
                accumulator.Add(value!);
            }
            else
            {
                //The optional first item failed, but if any input was consumed
                //then we must treat that as a failure rather than 'zero' items.

                if (originalIndex != index)
                {
                    expectation = itemExpectation;
                    succeeded = false;
                    return null;
                }

                expectation = null;
                succeeded = true;
                return accumulator;
            }

            var separatorAndNextItem =
                from _ in separator
                from next in item
                select next;

            return Repeat(accumulator, separatorAndNextItem, input, ref index, out succeeded, out expectation);
        };
    }

    /// <summary>
    /// OneOrMore(p, s) behaves like ZeroOrMore(p, s), except that p must succeed at least one time.
    /// </summary>
    public static Parser<TItem, IReadOnlyList<TValue>> OneOrMore<TItem, TValue, S>(Parser<TItem, TValue> item, Parser<TItem, S> separator)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeeded, out string? expectation) =>
        {
            var accumulator = new List<TValue>();

            var value = item(input, ref index, out var itemSucceeded, out var itemExpectation);

            if (itemSucceeded)
            {
                accumulator.Add(value!);
            }
            else
            {
                //The required first item failed.
                expectation = itemExpectation;
                succeeeded = false;
                return null;
            }

            var separatorAndNextItem =
                from _ in separator
                from next in item
                select next;

            return Repeat(accumulator, separatorAndNextItem, input, ref index, out succeeeded, out expectation);
        };
    }

    static IReadOnlyList<TValue>? Repeat<TItem, TValue>(List<TValue> accumulator,
                                                        Parser<TItem, TValue> item,
                                                        ReadOnlySpan<TItem> input,
                                                        ref int index,
                                                        out bool succeeded,
                                                        out string? expectation)
    {
        var oldIndex = index;

        var itemValue = item(input, ref index, out var itemSucceeded, out var itemExpectation);

        while (itemSucceeded)
        {
            if (oldIndex == index)
                throw new Exception($"Parser encountered a potential infinite loop at index {index}.");

            accumulator.Add(itemValue!);

            oldIndex = index;

            itemValue = item(input, ref index, out itemSucceeded, out itemExpectation);
        }

        //The item parser finally failed.

        if (oldIndex != index)
        {
            expectation = itemExpectation;
            succeeded = false;
            return null;
        }

        expectation = null;
        succeeded = true;
        return accumulator;
    }

    public static Parser<TItem, Void> Skip<TItem>(Func<TItem, bool> test)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var length = input.CountWhile(index, test);
            
            if (length > 0)
            {
                index += length;

                expectation = null;
                succeeded = true;
                return Void.Value;
            }

            expectation = null;
            succeeded = true;
            return Void.Value;
        };
    }

    public static Parser<char, string> ZeroOrMore(Func<char, bool> test)
    {
        return (ReadOnlySpan<char> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var length = input.CountWhile(index, test);
            
            if (length > 0)
            {
                var slice = input.Slice(index, length);
                index += length;

                expectation = null;
                succeeded = true;
                return slice.ToString();
            }

            expectation = null;
            succeeded = true;
            return "";
        };
    }

    public static Parser<char, string> OneOrMore(Func<char, bool> test, string name)
    {
        return (ReadOnlySpan<char> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var length = input.CountWhile(index, test);

            if (length > 0)
            {
                var span = input.Slice(index, length);
                index += length;

                expectation = null;
                succeeded = true;
                return span.ToString();
            }

            expectation = name;
            succeeded = false;
            return null;
        };
    }

    public static Parser<TItem, IReadOnlyList<TItem>> ZeroOrMore<TItem>(Func<TItem, bool> test)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var length = input.CountWhile(index, test);
            
            if (length > 0)
            {
                var slice = input.Slice(index, length);
                index += length;

                expectation = null;
                succeeded = true;
                return slice.ToArray();
            }

            expectation = null;
            succeeded = true;
            return Array.Empty<TItem>();
        };
    }

    public static Parser<TItem, IReadOnlyList<TItem>> OneOrMore<TItem>(Func<TItem, bool> test, string name)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var length = input.CountWhile(index, test);

            if (length > 0)
            {
                var span = input.Slice(index, length);
                index += length;

                expectation = null;
                succeeded = true;
                return span.ToArray();
            }

            expectation = name;
            succeeded = false;
            return null;
        };
    }
}

