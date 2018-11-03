using System;

namespace Parsley.Parsers
{
    public class BetweenParser<TItem> : Parser<TItem>
    {
        public BetweenParser(IGeneralParser left, IParser<TItem> item, IGeneralParser right)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public override IReply<TItem> Parse(TokenStream tokens)
        {
            var left = _left.ParseGeneral(tokens);

            if (!left.Success)
                return Error<TItem>.From(left);

            var item = _item.Parse(left.UnparsedTokens);

            if (!item.Success)
                return item;

            var right = _right.ParseGeneral(item.UnparsedTokens);

            if (!right.Success)
                return Error<TItem>.From(right);

            return new Parsed<TItem>(item.Value, right.UnparsedTokens, right.ErrorMessages);
        }

        public override IGeneralReply ParseGeneral(TokenStream tokens)
        {
            var left = _left.ParseGeneral(tokens);

            if (!left.Success)
                return left;

            var item = _item.ParseGeneral(left.UnparsedTokens);

            if (!item.Success)
                return item;

            return _right.ParseGeneral(item.UnparsedTokens);
        }

        protected override string GetName() => $"<({_left.Name}|{_item.Name}|{_right.Name})>";
        
        private readonly IGeneralParser _left;
        private readonly IParser<TItem> _item;
        private readonly IGeneralParser _right;
    }
}
