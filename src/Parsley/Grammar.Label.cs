using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// When parser p consumes any input, Label(p, l) is the same as p.
    /// When parser p does not consume any input, Label(p, l) is the same
    /// as p, except any messages are replaced with expectation label l.
    /// </summary>
    public static Parser<char, TValue> Label<TValue>(Parser<char, TValue> parse, string label)
    {
        return (ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out TValue? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var originalIndex = index;

            if (parse(input, ref index, out value, out expectation))
                return true;

            if (originalIndex == index)
                expectation = label;

            return false;
        };
    }
}
