using System;

namespace Parsley.Parsers
{
    public class TokenByKindParser : Parser
    {
        private readonly TokenKind _kind;

        public TokenByKindParser(TokenKind kind)
            => _kind = kind ?? throw new ArgumentNullException(nameof(kind));

        public override IReplyG ParseG(TokenStream tokens)
        {
            if (tokens.Current.Kind == _kind)
                return new ParsedG(tokens.Advance());

            return new ErrorG(tokens, ErrorMessage.Expected(_kind.Name));
        }

        protected override string GetName() => $"<*{_kind}*>";
    }
}