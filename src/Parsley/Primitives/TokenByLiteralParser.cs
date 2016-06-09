namespace Parsley.Primitives
{
    internal class TokenByLiteralParser : Parser<Token>
    {
        private readonly string expectation;

        public TokenByLiteralParser(string expectation)
        {
            this.expectation = expectation;
        }

        public Reply<Token> Parse(TokenStream tokens)
        {
            if (tokens.Current.Literal == expectation)
                return new Parsed<Token>(tokens.Current, tokens.Advance());

            return new Error<Token>(tokens, ErrorMessage.Expected(expectation));
        }
    }
}