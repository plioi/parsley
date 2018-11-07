using System;

namespace Parsley.Parsers
{
    public class TakeSkipParser<TResult> : Parser<TResult>
    {
        public TakeSkipParser(IParser<TResult> take, IParserG skip)
        {
            _take = take ?? throw new ArgumentNullException(nameof(take));
            _skip = skip ?? throw new ArgumentNullException(nameof(skip));
        }

        public override IReply<TResult> Parse(TokenStream tokens)
        {
            var take = _take.Parse(tokens);

            if (!take.Success)
                return Error<TResult>.From(take);

            var skip = _skip.ParseG(take.UnparsedTokens);

            if (!skip.Success)
                return Error<TResult>.From(skip);

            return new Parsed<TResult>(take.Value, skip.UnparsedTokens, skip.ErrorMessages);
        }

        public override IReplyG ParseG(TokenStream tokens)
        {
            var take = _take.ParseG(tokens);

            if (!take.Success)
                return take;

            return _skip.ParseG(take.UnparsedTokens);
        }

        protected override string GetName() => $"<TAKE {_take.Name} SKIP {_skip.Name}>";

        private readonly IParser<TResult> _take;
        private readonly IParserG _skip;
    }
}
