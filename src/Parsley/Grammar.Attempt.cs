using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// The parser Attempt(p) behaves like parser p, except that it pretends
    /// that it hasn't consumed any input when an error occurs. This combinator
    /// is used whenever arbitrary look ahead is needed.
    /// </summary>
    public static Parser<TValue> Attempt<TValue>(Parser<TValue> parse)
    {
        return (ref ReadOnlySpan<char> input, ref Index position, [NotNullWhen(true)] out TValue? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var originalInput = input;
            var originalPosition = position;

            if (parse(ref input, ref position, out value, out expectation))
                return true;

            if (originalInput != input)
            {
                input = originalInput;
                position = originalPosition;
            }

            return false;
        };
    }
}
