namespace Parsley;

public static class ParserQuery
{
    /// <summary>
    /// Converts any value into a parser that always succeeds with the given value.
    /// </summary>
    /// <remarks>
    /// In monadic terms, this is the 'Unit' function.
    /// </remarks>
    /// <typeparam name="TItem">The type of the items in the span being traversed.</typeparam>
    /// <typeparam name="TValue">The type of the value to treat as a parse result.</typeparam>
    /// <param name="value">The value to treat as a parse result.</param>
    public static Parser<TItem, TValue> SucceedWithThisValue<TItem, TValue>(this TValue value)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            expectation = null;
            succeeded = true;
            return value!;
        };
    }

    /// <summary>
    /// Allows LINQ syntax to construct a new parser from a simpler parser, using a single 'from' clause.
    /// </summary>
    public static Parser<TItem, U> Select<TItem, T, U>(this Parser<TItem, T> parser, Func<T, U> constructResult)
    {
        return parser.Bind(t => constructResult(t).SucceedWithThisValue<TItem, U>());
    }

    /// <summary>
    /// Allows LINQ syntax to construct a new parser from an ordered sequence of simpler parsers, using multiple 'from' clauses.
    /// </summary>
    public static Parser<TItem, V> SelectMany<TItem, T, U, V>(this Parser<TItem, T> parser, Func<T, Parser<TItem, U>> k, Func<T, U, V> s)
    {
        return parser.Bind(x => k(x).Bind(y => s(x, y).SucceedWithThisValue<TItem, V>()));
    }

    /// <summary>
    /// Extend a parser such that, after executing, the remaining input is processed by the next parser in the chain.
    /// </summary>
    /// <remarks>
    /// In monadic terms, this is the 'Bind' function.
    /// </remarks>
    static Parser<TItem, U> Bind<TItem, T, U>(this Parser<TItem, T> parse, Func<T, Parser<TItem, U>> constructNextParser)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var tValue = parse(input, ref index, out var tSucceeded, out expectation);

            if (tSucceeded)
                return constructNextParser(tValue!)(input, ref index, out succeeded, out expectation);

            succeeded = false;
            return default;
        };
    }
}
