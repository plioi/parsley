using System;

namespace Parsley.Parsers
{
    public class MapTokenLiteralParser<TResult> : Parser<TResult>
    {
        public MapTokenLiteralParser(TokenKind kind, Func<string, TResult> resultContinuation)
        {
            _kind = kind;
            _resultContinuation = resultContinuation ?? throw new ArgumentNullException(nameof(resultContinuation));
        }

        public override IReply<TResult> Parse(TokenStream tokens)
        {
            if (tokens.Current.Kind != _kind)
                return new Error<TResult>(tokens, ErrorMessage.Expected(_kind.Name));

            var parsedValue = _resultContinuation(tokens.Current.Literal);

            return new Parsed<TResult>(parsedValue, tokens.Advance());
        }

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed. NOTE: Result continuation will not be called.
        /// </summary>
        /// <param name="tokens">Tokens to parse</param>
        public override IGeneralReply ParseGeneral(TokenStream tokens)
        {
            if (tokens.Current.Kind != _kind)
                return new ErrorGeneral(tokens, ErrorMessage.Expected(_kind.Name));

            return new ParsedGeneral(tokens.Advance());
        }

        protected override string GetName() => $"<BTL *{_kind}* TO {typeof(TResult)}>";

        private readonly TokenKind _kind;
        private readonly Func<string, TResult> _resultContinuation;
    }
}
