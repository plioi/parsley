namespace Parsley;

partial class Grammar
{
    public static Parser<string> Operator(string symbol)
    {
        return input =>
        {
            var peek = input.Peek(symbol.Length);

            if (peek == symbol)
            {
                input.Advance(peek.Length);

                return new Parsed<string>(peek, input.Position, input.EndOfInput);
            }

            return new Error<string>(input.Position, input.EndOfInput, ErrorMessage.Expected(symbol));
        };
    }
}
