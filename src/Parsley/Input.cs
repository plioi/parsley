namespace Parsley;

public class Input
{
    readonly IReadOnlyList<Token> tokens;
    readonly int currentIndex;

    public Input(IEnumerable<Token> tokens)
    {
        this.tokens = tokens.ToList();

        if (this.tokens.Count == 0)
            this.tokens = new[] { new Token(TokenKind.EndOfInput, new Position(1, 1), "") };

        currentIndex = 0;
    }

    Input(IReadOnlyList<Token> tokens, int currentIndex)
    {
        this.tokens = tokens;
        this.currentIndex = currentIndex;
    }

    public Token Current => tokens[currentIndex];

    public Position Position => Current.Position;

    public Input Advance()
    {
        if (currentIndex < tokens.Count - 1)
            return new Input(tokens, currentIndex + 1);

        if (Current.Kind == TokenKind.EndOfInput)
            return this;

        var endPosition = new Position(Position.Line, Position.Column + Current.Literal.Length);

        return new Input(new[] { new Token(TokenKind.EndOfInput, endPosition, "") });
    }
}
