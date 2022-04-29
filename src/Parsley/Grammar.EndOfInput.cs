using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    public static Parser<char, Void> EndOfInput()
    {
        return (ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out Void value, [NotNullWhen(false)] out string? expectation) =>
        {
            value = Void.Value;

            if (index == input.Length)
            {
                expectation = null;

                return true;
            }

            expectation = "end of input";

            return false;
        };
    }
}
