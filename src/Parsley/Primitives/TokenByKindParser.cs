namespace Parsley.Primitives
{
    internal class TokenByKindParser : Parser<Token>
    {
        private readonly TokenKind kind;

        public TokenByKindParser(TokenKind kind)
        {
            this.kind = kind;
        }

        public Reply<Token> Parse(TokenStream tokens)
        {
            if (tokens.Current.Kind == kind)
                return new Parsed<Token>(tokens.Current, tokens.Advance());

            return new Error<Token>(tokens, ErrorMessage.Expected(kind.Name));
        }
    }
}