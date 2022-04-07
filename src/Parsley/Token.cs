namespace Parsley;

public record Token(TokenKind Kind, Position Position, string Literal)
{
    public override string ToString()
        => $"Kind: {Kind}, Position: {Position}, Literal: {Literal}";
}
