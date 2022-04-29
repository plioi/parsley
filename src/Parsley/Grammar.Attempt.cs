using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// The parser Attempt(p) behaves like parser p, except that it pretends
    /// that it hasn't consumed any input when an error occurs. This combinator
    /// is used whenever arbitrary look ahead is needed.
    /// </summary>
    public static Parser<char, TValue> Attempt<TValue>(Parser<char, TValue> parse)
    {
        return (ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out TValue? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var originalIndex = index;

            if (parse(input, ref index, out value, out expectation))
                return true;

            index = originalIndex;

            return false;
        };
    }
}
