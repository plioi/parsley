using System;

namespace Parsley.Parsers
{
    public class MonadicBindParser<TInterim, TResult> : Parser<TResult>
    {
        public MonadicBindParser(IParser<TInterim> parser, Func<TInterim, TResult> resultContinuation)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _resultContinuation = resultContinuation ?? throw new ArgumentNullException(nameof(resultContinuation));
        }

        public override IReply<TResult> Parse(TokenStream tokens)
        {
            var reply = _parser.Parse(tokens);

            if (!reply.Success)
                return Error<TResult>.From(reply);

            var parsedValue = _resultContinuation(reply.Value);

            return new Parsed<TResult>(parsedValue, reply.UnparsedTokens);
        }

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed. NOTE: Result continuation will not be called.
        /// </summary>
        /// <param name="tokens">Tokens to parse</param>
        public override IGeneralReply ParseGeneral(TokenStream tokens) => _parser.ParseGeneral(tokens);

        protected override string GetName() => $"<BIND {_parser.Name} TO {typeof(TResult)}>";
        
        private readonly IParser<TInterim> _parser;
        private readonly Func<TInterim, TResult> _resultContinuation;
    }

    public class MonadicBindParser<T1, T2, TResult> : Parser<TResult>
    {
        public MonadicBindParser(IParser<T1> parser1, Func<T1, IParser<T2>> parser2Continuation,
            Func<T1, T2, TResult> resultContinuation)
        {
            _parser1 = parser1 ?? throw new ArgumentNullException(nameof(parser1));

            _parser2Continuation = parser2Continuation ?? throw new ArgumentNullException(nameof(parser2Continuation));
            _resultContinuation = resultContinuation ?? throw new ArgumentNullException(nameof(resultContinuation));
        }

        public override IReply<TResult> Parse(TokenStream tokens)
        {
            var reply1 = _parser1.Parse(tokens);

            if (!reply1.Success)
                return Error<TResult>.From(reply1);

            var value1 = reply1.Value;

            var parser2 = _parser2Continuation(value1);

            var reply2 = parser2.Parse(reply1.UnparsedTokens);

            if (!reply2.Success)
                return Error<TResult>.From(reply2);

            var value2 = reply2.Value;

            var result = _resultContinuation(value1, value2);

            return new Parsed<TResult>(result, reply2.UnparsedTokens);
        }

        protected override string GetName() => $"<BIND2 {_parser1} TO {typeof(T1)} TO {typeof(T2)}>";

        private readonly IParser<T1> _parser1;

        private readonly Func<T1, IParser<T2>> _parser2Continuation;
        private readonly Func<T1, T2, TResult> _resultContinuation;
    }
}
