namespace Parsley.Parsers
{
    public class TokenByKindGeneralParser : Parser
    {
        private readonly TokenKind _kind;

        public TokenByKindGeneralParser(TokenKind kind)
        {
            _kind = kind;
        }

        protected override string GetName() => $"<*{_kind}*>";

        public override IReplyG ParseG(TokenStream tokens)
        {
            if (tokens.Current.Kind == _kind)
                return new ParsedG(tokens.Advance());

            return new ErrorG(tokens, ErrorMessage.Expected(_kind.Name));
        }
    }
}
