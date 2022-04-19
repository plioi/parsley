using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// The parser Attempt(p) behaves like parser p, except that it pretends
    /// that it hasn't consumed any input when an error occurs. This combinator
    /// is used whenever arbitrary look ahead is needed.
    /// </summary>
    public static Parser<T> Attempt<T>(Parser<T> parse)
    {
        return (ref ReadOnlySpan<char> input, ref Position position, [NotNullWhen(true)] out T? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var snapshot = input;
            var originalPosition = position;

            if (parse(ref input, ref position, out value, out expectation))
                return true;

            if (originalPosition != position)
            {
                input = snapshot;
                position = originalPosition;
            }

            return false;
        };
    }
}
