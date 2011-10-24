namespace Parsley.Primitives
{
    internal class TokenByLiteralParser : Parser<Token>
    {
        private readonly string expectation;

        public TokenByLiteralParser(string expectation)
        {
            this.expectation = expectation;
        }

        public Reply<Token> Parse(Lexer tokens)
        {
            if (tokens.CurrentToken.Literal == expectation)
                return new Parsed<Token>(tokens.CurrentToken, tokens.Advance());

            return new Error<Token>(tokens, ErrorMessage.Expected(expectation));
        }
    }
}