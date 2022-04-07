namespace Parsley.Primitives;

class TokenByKindParser : IParser<Token>
{
    readonly TokenKind kind;

    public TokenByKindParser(TokenKind kind)
    {
        this.kind = kind;
    }

    public Reply<Token> Parse(Input input)
    {
        if (input.Current.Kind == kind)
            return new Parsed<Token>(input.Current, input.Advance());

        return new Error<Token>(input, ErrorMessage.Expected(kind.Name));
    }
}
