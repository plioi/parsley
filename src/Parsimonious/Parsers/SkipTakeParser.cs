using System;

namespace Parsimonious.Parsers
{
    public class SkipTakeParser<TResult> : Parser<TResult>
    {
        public SkipTakeParser(IParserG skip, IParser<TResult> take)
        {
            _take = take ?? throw new ArgumentNullException(nameof(take));
            _skip = skip ?? throw new ArgumentNullException(nameof(skip));
        }

        public override IReply<TResult> Parse(TokenStream tokens)
        {
            var skip = _skip.ParseG(tokens);

            if (!skip.Success)
                return Error<TResult>.From(skip);

            return _take.Parse(skip.UnparsedTokens);
        }

        public override IReplyG ParseG(TokenStream tokens)
        {
            var skip = _skip.ParseG(tokens);

            if (!skip.Success)
                return skip;

            return _take.ParseG(skip.UnparsedTokens);
        }

        protected override string GetName() => $"<TAKE {_take.Name} SKIP {_skip.Name}>";

        private readonly IParser<TResult> _take;
        private readonly IParserG _skip;
    }
}
