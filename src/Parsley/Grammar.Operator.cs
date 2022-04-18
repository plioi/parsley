namespace Parsley;

partial class Grammar
{
    public static Parser<string> Operator(string symbol)
    {
        return (ref Text input) =>
        {
            var peek = input.Peek(symbol.Length);

            if (peek.Equals(symbol, StringComparison.Ordinal))
            {
                input.Advance(symbol.Length);

                return new Parsed<string>(symbol);
            }

            return new Error<string>(symbol);
        };
    }
}
