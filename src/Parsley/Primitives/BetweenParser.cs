using System;

namespace Parsley.Primitives
{
    public class BetweenParser<TLeft, TItem, TRight> : IParser<TItem>
    {
        public BetweenParser(IParser<TLeft> left, IParser<TItem> item, IParser<TRight> right)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public Reply<TItem> Parse(TokenStream tokens)
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

        public override string ToString()
        {
            return $"<({_left}|{_item}|{_right})>";
        }

        private readonly IParser<TLeft> _left;
        private readonly IParser<TItem> _item;
        private readonly IParser<TRight> _right;
    }
}
