using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    public static Parser<string> Operator(string symbol)
    {
        return (ref ReadOnlySpan<char> input, ref int position, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var peek = input.Peek(symbol.Length);

            if (peek.Equals(symbol, StringComparison.Ordinal))
            {
                input.Advance(ref position, symbol.Length);

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
