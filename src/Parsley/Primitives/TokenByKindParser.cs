namespace Parsley.Primitives;

class TokenByKindParser : IParser<Token>
{
    readonly TokenKind kind;

    public TokenByKindParser(TokenKind kind)
    {
        this.kind = kind;
    }

    public Reply<Token> Parse(Text input)
    {
        if (kind.TryMatch(input, out var token))
            return new Parsed<Token>(token, input.Advance(token.Literal.Length));

        return new Error<Token>(input, ErrorMessage.Expected(kind.Name));
    }
}
