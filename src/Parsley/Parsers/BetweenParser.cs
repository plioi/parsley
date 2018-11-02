using System;

namespace Parsley.Parsers
{
    public class BetweenParser<TLeft, TItem, TRight> : Parser<TItem>
    {
        public BetweenParser(IParser<TLeft> left, IParser<TItem> item, IParser<TRight> right)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public override IReply<TItem> Parse(TokenStream tokens)
        {
            var left = _left.Parse(tokens);

            if (!left.Success)
                return Error<TItem>.From(left);

            var item = _item.Parse(left.UnparsedTokens);

            if (!item.Success)
                return item;

            var right = _right.Parse(item.UnparsedTokens);

            if (!right.Success)
                return Error<TItem>.From(right);

            return new Parsed<TItem>(item.Value, right.UnparsedTokens, right.ErrorMessages);
        }

        protected override string GetName() => $"<({_left.Name}|{_item.Name}|{_right.Name})>";
        
        private readonly IParser<TLeft> _left;
        private readonly IParser<TItem> _item;
        private readonly IParser<TRight> _right;
    }
}
