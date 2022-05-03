namespace Parsley;

partial class Grammar
{
    public static Parser<char, string> Operator(string symbol)
    {
        return (ReadOnlySpan<char> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var peek = input.Peek(index, symbol.Length);

            if (peek.Equals(symbol, StringComparison.Ordinal))
            {
                index += symbol.Length;

                succeeded = true;
                expectation = null;
                return symbol;
            }

            succeeded = false;
            expectation = symbol;
            return null;
        };
    }
}
