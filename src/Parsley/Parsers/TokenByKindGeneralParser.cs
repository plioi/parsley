namespace Parsley.Parsers
{
    public class TokenByKindGeneralParser : IGeneralParser
    {
        private readonly TokenKind _kind;

        public TokenByKindGeneralParser(TokenKind kind)
        {
            _kind = kind;
        }

        public string Name => $"<*{_kind}*>";

        public IGeneralReply ParseGeneral(TokenStream tokens)
        {
            if (tokens.Current.Kind == _kind)
                return new ParsedGeneral(tokens.Advance());

            return new ErrorGeneral(tokens, ErrorMessage.Expected(_kind.Name));
        }
    }
}
