using System;

namespace Parsimonious.Parsers
{
    public class BindTokenLiteralParser<TResult> : Parser<TResult>
    {
        public BindTokenLiteralParser(IParser<Token> parser, Func<string, TResult> resultContinuation)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _resultContinuation = resultContinuation ?? throw new ArgumentNullException(nameof(resultContinuation));
        }

        public override IReply<TResult> Parse(TokenStream tokens)
        {
            var reply = _parser.Parse(tokens);

            if (!reply.Success)
                return Error<TResult>.From(reply);

            var parsedValue = _resultContinuation(reply.Value.Literal);

            return new Parsed<TResult>(parsedValue, reply.UnparsedTokens);
        }

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed. NOTE: Result continuation will not be called.
        /// </summary>
        /// <param name="tokens">Tokens to parse</param>
        public override IReplyG ParseG(TokenStream tokens) => _parser.ParseG(tokens);

        protected override string GetName() => $"<BTL {_parser.Name} TO {typeof(TResult)}>";
        
        private readonly IParser<Token> _parser;
        private readonly Func<string, TResult> _resultContinuation;
    }
}
