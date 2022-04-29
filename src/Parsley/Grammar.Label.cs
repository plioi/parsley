using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// When parser p consumes any input, Label(p, l) is the same as p.
    /// When parser p does not consume any input, Label(p, l) is the same
    /// as p, except any messages are replaced with expectation label l.
    /// </summary>
    public static Parser<TValue> Label<TValue>(Parser<TValue> parse, string label)
    {
        return (ref ReadOnlySpan<char> input, ref @int position, [NotNullWhen(true)] out TValue? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var originalInput = input;

            if (parse(ref input, ref position, out value, out expectation))
                return true;

            if (originalInput == input)
                expectation = label;

            return false;
        };
    }
}
