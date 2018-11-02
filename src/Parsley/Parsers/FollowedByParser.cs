using System;

namespace Parsley.Parsers
{
    public class FollowedByParser<TResult, TDummy> : Parser<TResult>
    {
        public FollowedByParser(IParser<TResult> item, IParser<TDummy> following)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _following = following ?? throw new ArgumentNullException(nameof(following));
        }

        public override IReply<TResult> Parse(TokenStream tokens)
        {
            var item = _item.Parse(tokens);

            if (!item.Success)
                return Error<TResult>.From(item);

            var following = _following.Parse(item.UnparsedTokens);

            if (!following.Success)
                return Error<TResult>.From(following);

            return new Parsed<TResult>(item.Value, following.UnparsedTokens, following.ErrorMessages);
        }

        protected override string GetName() => $"<{_item.Name} FOLLOWED BY {_following.Name}>";

        private readonly IParser<TResult> _item;
        private readonly IParser<TDummy> _following;
    }
}
