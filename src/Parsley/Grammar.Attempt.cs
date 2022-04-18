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
        return (ref Text input, [NotNullWhen(true)] out T? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var snapshot = input;
            var start = input.Position;

            if (parse(ref input, out value, out expectation))
                return true;

            var newPosition = input.Position;

            if (start != newPosition)
                input = snapshot;

            return false;
        };
    }
}
