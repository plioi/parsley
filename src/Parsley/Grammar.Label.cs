using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// When parser p consumes any input, Label(p, l) is the same as p.
    /// When parser p does not consume any input, Label(p, l) is the same
    /// as p, except any messages are replaced with expectation label l.
    /// </summary>
    public static Parser<T> Label<T>(Parser<T> parse, string label)
    {
        return (ref Text input, [NotNullWhen(true)] out T? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var start = input.Position;

            if (parse(ref input, out value, out expectation))
                return true;

            var newPosition = input.Position;

            if (start == newPosition)
            {
                expectation = label;
                value = default;
            }

            return false;
        };
    }
}
