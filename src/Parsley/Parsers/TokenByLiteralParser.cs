namespace Parsley.Parsers
{
    public class TokenByLiteralParser : Parser<Token>
    {
        private readonly string _expectation;

        public TokenByLiteralParser(string expectation)
        {
            _expectation = expectation;
        }

        public override IReply<Token> Parse(TokenStream tokens)
        {
            if (tokens.Current.Literal == _expectation)
                return new Parsed<Token>(tokens.Current, tokens.Advance());

            return new Error<Token>(tokens, ErrorMessage.Expected(_expectation));
        }

        protected override string GetName() => $"<'{_expectation}'>";
    }
}