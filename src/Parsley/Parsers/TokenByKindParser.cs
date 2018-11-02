namespace Parsley.Parsers
{
    public class TokenByKindParser : Parser<Token>
    {
        private readonly TokenKind _kind;

        public TokenByKindParser(TokenKind kind)
        {
            _kind = kind;
        }

        public override IReply<Token> Parse(TokenStream tokens)
        {
            if (tokens.Current.Kind == _kind)
                return new Parsed<Token>(tokens.Current, tokens.Advance());

            return new Error<Token>(tokens, ErrorMessage.Expected(_kind.Name));
        }

        protected override string GetName() => $"<T {_kind}>";
    }
}