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

    public Reply<Token> Parse(Input input)
    {
        if (input.Current.Kind == kind && input.Current.Literal == expectation)
            return new Parsed<Token>(input.Current, input.Advance());

        return new Error<Token>(input, ErrorMessage.Expected(expectation));
    }
}
