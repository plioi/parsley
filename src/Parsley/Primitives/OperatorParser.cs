namespace Parsley.Primitives;

public class OperatorParser : IParser<string>
{
    readonly string symbol;

    public OperatorParser(string symbol)
        => this.symbol = symbol;

    public Reply<string> Parse(Text input)
    {
        var peek = input.Peek(symbol.Length);

        return peek == symbol
            ? new Parsed<string>(peek, input.Advance(peek.Length))
            : new Error<string>(input, ErrorMessage.Expected(symbol));
    }
}
