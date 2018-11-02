using System;

namespace Parsley.Parsers
{
    public class BindTokenLiteralParser<TResult> : IParser<TResult>
    {
        public BindTokenLiteralParser(IParser<Token> parser, Func<string, TResult> resultContinuation)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _resultContinuation = resultContinuation ?? throw new ArgumentNullException(nameof(resultContinuation));
        }

        Reply<TResult> IParser<TResult>.Parse(TokenStream tokens)
        {
            var reply = _parser.Parse(tokens);

            if (!reply.Success)
                return Error<TResult>.From(reply);

            var parsedValue = _resultContinuation(reply.Value.Literal);

            return new Parsed<TResult>(parsedValue, reply.UnparsedTokens);
        }

        private readonly IParser<Token> _parser;
        private readonly Func<string, TResult> _resultContinuation;

        public override string ToString() => $"<BTL {_parser.Name} TO {typeof(TResult)}>";
        public string Name => ToString();
    }
}
