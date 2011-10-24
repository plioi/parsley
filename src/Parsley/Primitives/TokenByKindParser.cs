namespace Parsley.Primitives
{
    internal class TokenByKindParser : Parser<Token>
    {
        private readonly TokenKind kind;

        public TokenByKindParser(TokenKind kind)
        {
            this.kind = kind;
        }

        public Reply<Token> Parse(Lexer tokens)
        {
            if (tokens.CurrentToken.Kind == kind)
                return new Parsed<Token>(tokens.CurrentToken, tokens.Advance());

            return new Error<Token>(tokens, ErrorMessage.Expected(kind.Name));
        }
    }
}