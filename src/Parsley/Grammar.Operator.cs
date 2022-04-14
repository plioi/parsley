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
                input.Advance(symbol.Length);

                return new Parsed<string>(symbol, input.Position);
            }

            return new Error<string>(input.Position, ErrorMessage.Expected(symbol));
        };
    }
}
