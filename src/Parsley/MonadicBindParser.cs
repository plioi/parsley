using System;

namespace Parsley
{
    public class MonadicBindParser<TInterim, TResult> : IParser<TResult>
    {
        public MonadicBindParser(IParser<TInterim> parser, Func<TInterim, TResult> resultContinuation)
        {
            _parser = parser;
            _resultContinuation = resultContinuation;
        }

        Reply<TResult> IParser<TResult>.Parse(TokenStream tokens)
        {
            var reply = _parser.Parse(tokens);

            if (!reply.Success)
                return new Error<TResult>(reply.UnparsedTokens, reply.ErrorMessages);

            var parsedValue = _resultContinuation(reply.Value);

            return new Parsed<TResult>(parsedValue, reply.UnparsedTokens);
        }

        private readonly IParser<TInterim> _parser;
        private readonly Func<TInterim, TResult> _resultContinuation;

        public override string ToString()
        {
            return $"<bind {_parser}>";
        }
    }

    public class MonadicBindParser<T1, T2, TResult> : IParser<TResult>
    {
        public MonadicBindParser(IParser<T1> parser1, Func<T1, IParser<T2>> parser2Continuation, Func<T1, T2, TResult> resultContinuation)
        {
            _parser1 = parser1;
            _parser2Continuation = parser2Continuation;
            _resultContinuation = resultContinuation;
        }

        private readonly IParser<T1> _parser1;
        private readonly Func<T1, IParser<T2>> _parser2Continuation;
        private readonly Func<T1, T2, TResult> _resultContinuation;

        Reply<TResult> IParser<TResult>.Parse(TokenStream tokens)
        {
            var reply1 = _parser1.Parse(tokens);

            if (!reply1.Success)
                return new Error<TResult>(reply1.UnparsedTokens, reply1.ErrorMessages);

            var value1 = reply1.Value;

            var parser2 = _parser2Continuation(value1);

            var reply2 = parser2.Parse(reply1.UnparsedTokens);

            if (!reply2.Success)
                return new Error<TResult>(reply2.UnparsedTokens, reply2.ErrorMessages);

            var value2 = reply2.Value;

            var result = _resultContinuation(value1, value2);

            return new Parsed<TResult>(result, reply2.UnparsedTokens);
        }

        public override string ToString()
        {
            return $"<bind {_parser1} to {_parser2Continuation}>";
        }
    }
}
