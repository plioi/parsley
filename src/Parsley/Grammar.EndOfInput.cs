using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    public static readonly Parser<string> EndOfInput =
        (ref ReadOnlySpan<char> input, ref Index position, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation) =>
        {
            if (input.IsEmpty)
            {
                expectation = null;
                value = "";
                return true;
            }

            expectation = "end of input";
            value = null;
            return false;
        };
}
