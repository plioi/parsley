using System.Diagnostics.CodeAnalysis;

namespace Parsley;

public static class ParserQuery
{
    /// <summary>
    /// Converts any value into a parser that always succeeds with the given value.
    /// </summary>
    /// <remarks>
    /// In monadic terms, this is the 'Unit' function.
    /// </remarks>
    /// <typeparam name="TValue">The type of the value to treat as a parse result.</typeparam>
    /// <param name="value">The value to treat as a parse result.</param>
    public static Parser<char, TValue> SucceedWithThisValue<TValue>(this TValue value)
    {
        return (ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out TValue? succeedingValue, [NotNullWhen(false)] out string? expectation) =>
        {
            expectation = null;
            succeedingValue = value!;
            return true;
        };
    }

    /// <summary>
    /// Allows LINQ syntax to construct a new parser from a simpler parser, using a single 'from' clause.
    /// </summary>
    public static Parser<char, U> Select<T, U>(this Parser<char, T> parser, Func<T, U> constructResult)
    {
        return parser.Bind(t => constructResult(t).SucceedWithThisValue<U>());
    }

    /// <summary>
    /// Allows LINQ syntax to construct a new parser from an ordered sequence of simpler parsers, using multiple 'from' clauses.
    /// </summary>
    public static Parser<char, V> SelectMany<T, U, V>(this Parser<char, T> parser, Func<T, Parser<char, U>> k, Func<T, U, V> s)
    {
        return parser.Bind(x => k(x).Bind(y => s(x, y).SucceedWithThisValue<V>()));
    }

    /// <summary>
    /// Extend a parser such that, after executing, the remaining input is processed by the next parser in the chain.
    /// </summary>
    /// <remarks>
    /// In monadic terms, this is the 'Bind' function.
    /// </remarks>
    static Parser<char, U> Bind<T, U>(this Parser<char, T> parse, Func<T, Parser<char, U>> constructNextParser)
    {
        return (ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out U? uValue, [NotNullWhen(false)] out string? expectation) =>
        {
            if (parse(input, ref index, out var tValue, out expectation))
                return constructNextParser(tValue)(input, ref index, out uValue, out expectation);

            uValue = default;
            return false;
        };
    }
}
