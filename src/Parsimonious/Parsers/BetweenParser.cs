using System;

namespace Parsimonious.Parsers
{
    public class BetweenParser<TItem> : Parser<TItem>
    {
        public BetweenParser(IParserG left, IParser<TItem> item, IParserG right)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public override IReply<TItem> Parse(TokenStream tokens)
        {
            var left = _left.ParseG(tokens);

            if (!left.Success)
                return Error<TItem>.From(left);

            var item = _item.Parse(left.UnparsedTokens);

            if (!item.Success)
                return item;

            var right = _right.ParseG(item.UnparsedTokens);

            if (!right.Success)
                return Error<TItem>.From(right);

            return new Parsed<TItem>(item.Value, right.UnparsedTokens, right.ErrorMessages);
        }

        public override IReplyG ParseG(TokenStream tokens)
        {
            var left = _left.ParseG(tokens);

            if (!left.Success)
                return left;

            var item = _item.ParseG(left.UnparsedTokens);

            if (!item.Success)
                return item;

            return _right.ParseG(item.UnparsedTokens);
        }

        protected override string GetName() => $"<({_left.Name}|{_item.Name}|{_right.Name})>";
        
        private readonly IParserG _left;
        private readonly IParser<TItem> _item;
        private readonly IParserG _right;
    }
}
