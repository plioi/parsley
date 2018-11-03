namespace Parsley.Parsers
{
    public class TokenByKindGeneralParser : IParserG
    {
        private readonly TokenKind _kind;

        public TokenByKindGeneralParser(TokenKind kind)
        {
            _kind = kind;
        }

        public string Name => $"<*{_kind}*>";

        public IReplyG ParseG(TokenStream tokens)
        {
            if (tokens.Current.Kind == _kind)
                return new ParsedG(tokens.Advance());

            return new ErrorG(tokens, ErrorMessage.Expected(_kind.Name));
        }
    }
}
