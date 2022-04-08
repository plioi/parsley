namespace Parsley.Primitives;

class TokenByLiteralParser : IParser<Token>
{
    readonly TokenKind kind;
    readonly string expectation;

    public TokenByLiteralParser(TokenKind kind, string expectation)
    {
        this.kind = kind;
        this.expectation = expectation;
    }

    public Reply<Token> Parse(Text input)
    {
        if (kind.TryMatch(input, out var token) && token.Literal == expectation)
            return new Parsed<Token>(token, input.Advance(token.Literal.Length));

        return new Error<Token>(input, ErrorMessage.Expected(expectation));
    }
}
