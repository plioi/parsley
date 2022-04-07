namespace Parsley;

public class Input
{
    readonly Token current;
    readonly Lazy<Input> rest;

    public Input(IEnumerable<Token> tokens)
    {
        var enumerator = tokens.GetEnumerator();

        current = enumerator.MoveNext()
            ? enumerator.Current
            : new Token(TokenKind.EndOfInput, new Position(1, 1), "");

        rest = new Lazy<Input>(() => LazyAdvance(enumerator));
    }

    Input(Token current, IEnumerator<Token> enumerator)
    {
        this.current = current;
        rest = new Lazy<Input>(() => LazyAdvance(enumerator));
    }

    Input(Token current)
    {
        this.current = current;
        rest = new Lazy<Input>(() => this);
    }

    public Token Current => current;

    public Input Advance() => rest.Value;

    public Position Position => Current.Position;

    Input LazyAdvance(IEnumerator<Token> enumerator)
    {
        if (enumerator.MoveNext())
            return new Input(enumerator.Current, enumerator);

        if (Current.Kind == TokenKind.EndOfInput)
            return this;

        var endPosition = new Position(Position.Line, Position.Column + Current.Literal.Length);

        return new Input(new Token(TokenKind.EndOfInput, endPosition, ""));
    }
}
