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
                var unparsedInput = input.Advance(peek.Length);

                return new Parsed<string>(peek, unparsedInput, unparsedInput.Position, unparsedInput.EndOfInput);
            }

            return new Error<string>(input, input.Position, input.EndOfInput, ErrorMessage.Expected(symbol));
        };
    }
}
