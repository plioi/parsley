using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    public static Parser<string> Operator(string symbol)
    {
        return (ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var peek = input.Peek(index, symbol.Length);

            if (peek.Equals(symbol, StringComparison.Ordinal))
            {
                index += symbol.Length;

                expectation = null;
                value = symbol;
                return true;
            }

            expectation = symbol;
            value = null;
            return false;
        };
    }
}
