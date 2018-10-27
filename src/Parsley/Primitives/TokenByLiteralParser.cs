namespace Parsley.Primitives
{
    public class TokenByLiteralParser : IParser<Token>
    {
        private readonly string _expectation;

        public TokenByLiteralParser(string expectation)
        {
            _expectation = expectation;
        }

        public Reply<Token> Parse(TokenStream tokens)
        {
            if (tokens.Current.Literal == _expectation)
                return new Parsed<Token>(tokens.Current, tokens.Advance());

            return new Error<Token>(tokens, ErrorMessage.Expected(_expectation));
        }

        public override string ToString() => $"<L {_expectation}>";
        public string Name => ToString();
    }
}