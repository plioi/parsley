namespace Parsley.Parsers
{
    public class TokenByKindParser : IParser<Token>
    {
        private readonly TokenKind _kind;

        public TokenByKindParser(TokenKind kind)
        {
            _kind = kind;
        }

        public Reply<Token> Parse(TokenStream tokens)
        {
            if (tokens.Current.Kind == _kind)
                return new Parsed<Token>(tokens.Current, tokens.Advance());

            return new Error<Token>(tokens, ErrorMessage.Expected(_kind.Name));
        }

        public override string ToString() => $"<T {_kind}>";
        public string Name => ToString();
    }
}