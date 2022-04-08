namespace Parsley;

public record Token(TokenKind Kind, string Literal)
{
    public override string ToString()
        => $"Kind: {Kind}, Literal: {Literal}";
}
