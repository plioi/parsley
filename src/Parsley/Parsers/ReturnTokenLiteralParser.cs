using System;

namespace Parsley.Parsers
{
    public class ReturnTokenLiteralParser : Parser<string>
    {
        public ReturnTokenLiteralParser(TokenKind kind)
        {
            _kind = kind ?? throw new ArgumentNullException(nameof(kind));
        }

        public override IReply<string> Parse(TokenStream tokens)
        {
            var currentToken = tokens.Current;

            if (currentToken.Kind != _kind)
                return new Error<string>(tokens, ErrorMessage.Expected(_kind.Name));

            return new Parsed<string>(currentToken.Literal, tokens.Advance());
        }

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed. NOTE: Result continuation will not be called.
        /// </summary>
        /// <param name="tokens">Tokens to parse</param>
        public override IReplyG ParseG(TokenStream tokens)
        {
            if (tokens.Current.Kind != _kind)
                return new ErrorG(tokens, ErrorMessage.Expected(_kind.Name));

            return new ParsedG(tokens.Advance());
        }

        protected override string GetName() => $"<L*{_kind}*>";

        private readonly TokenKind _kind;
    }
}
