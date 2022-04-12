namespace Parsley;

partial class Grammar
{
    public static Parser<string> Operator(string symbol)
    {
        return input =>
        {
            var peek = input.Peek(symbol.Length);

            return peek == symbol
                ? new Parsed<string>(peek, input.Advance(peek.Length))
                : new Error<string>(input, ErrorMessage.Expected(symbol));
        };
    }
}
