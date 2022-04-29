using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    public static Parser<char, string> EndOfInput()
    {
        return (ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation) =>
        {
            if (index == input.Length)
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
}
