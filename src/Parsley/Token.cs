namespace Parsley;

public record Token(IParser<Token> Kind, string Literal)
{
    public override string ToString()
        => $"Kind: {Kind}, Literal: {Literal}";
}
