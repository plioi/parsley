using System;
using System.Collections.Generic;

namespace Parsley.Parsers
{
    public enum QuantificationRule
    {
        NOrMore,
        ExactlyN,
        NOrLess,
        NtoM
    }

    public class QuantifiedParser<TItem, TSeparator> : IParser<IList<TItem>>
    {
        private readonly IParser<TItem> _item;
        private readonly QuantificationRule _quantificationRule;
        private readonly int _n;
        private readonly int _m;
        private readonly IParser<TSeparator> _itemSeparator;
        
        public QuantifiedParser(IParser<TItem> item, QuantificationRule quantificationRule, int n, int m = -1, IParser<TSeparator> itemSeparator = null)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));

            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n), "should be non-negative");

            switch (quantificationRule)
            {
                case QuantificationRule.ExactlyN:
                case QuantificationRule.NOrMore:
                    if (m != -1)
                        throw new ArgumentOutOfRangeException(nameof(m), "this value is not used in this mode and should be left -1");
                    break;
                case QuantificationRule.NtoM:
                    if (n > m)
                        throw new ArgumentOutOfRangeException(nameof(m), "should not be less than nTimes");
                    break;
            }

            if (item == itemSeparator)
                throw new ArgumentException("parser for the item and the separator cannot be the same one", nameof(itemSeparator));

            _quantificationRule = quantificationRule;

            _n = n;
            _m = m;

            _itemSeparator = itemSeparator;
        }

        public Reply<IList<TItem>> Parse(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _item.Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            var times = 0;

            var list = new List<TItem>();

            var separatorParserIsPresent = _itemSeparator != null;
            var separatorWasParsed = false;

            while (reply.Success)
            {
                if (oldPosition == newPosition)
                    throw new Exception($"Item parser {_item} encountered a potential infinite loop at position {newPosition}.");

                ++times;

                switch (_quantificationRule)
                {
                    case QuantificationRule.ExactlyN:
                        if (times > _n)
                            return new Error<IList<TItem>>(
                                reply.UnparsedTokens,
                                ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occurring no more than exactly {_n} times"))
                            );
                        break;
                    case QuantificationRule.NtoM:
                        if (times > _m)
                            return new Error<IList<TItem>>(
                                reply.UnparsedTokens,
                                ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occurring no more than between {_n} and {_m} times"))
                            );
                        break;
                    case QuantificationRule.NOrLess:
                        if (times > _n)
                            return new Error<IList<TItem>>(
                                reply.UnparsedTokens,
                                ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occurring no more than {_n} times"))
                            );
                        break;
                }

                list.Add(reply.Value);

                var unparsedTokens = reply.UnparsedTokens;

                if (separatorParserIsPresent)
                {
                    var o = newPosition;

                    var r = _itemSeparator.Parse(reply.UnparsedTokens);

                    unparsedTokens = r.UnparsedTokens;
                    newPosition = unparsedTokens.Position;

                    if (r.Success && o == newPosition)
                        throw new Exception($"Separator parser {_itemSeparator} encountered a potential infinite loop at position {newPosition}.");

                    separatorWasParsed = r.Success;
                }

                oldPosition = newPosition;

                if (separatorParserIsPresent && !separatorWasParsed)
                    break;

                reply = _item.Parse(unparsedTokens);

                if (!reply.Success && separatorParserIsPresent)
                    return new Error<IList<TItem>>(reply.UnparsedTokens, reply.ErrorMessages);

                newPosition = reply.UnparsedTokens.Position;
            }

            //The item parser finally failed or the separator parser parsed the next separator, but there was no item following it
            if (oldPosition != newPosition || separatorParserIsPresent && separatorWasParsed)
                return new Error<IList<TItem>>(reply.UnparsedTokens, reply.ErrorMessages);

            switch (_quantificationRule)
            {
                case QuantificationRule.NOrMore:
                    if (times < _n)
                        return new Error<IList<TItem>>(
                            reply.UnparsedTokens,
                            ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occurring {_n}+ times"))
                        );
                    break;
                case QuantificationRule.ExactlyN:
                    if (times != _n)
                        return new Error<IList<TItem>>(
                            reply.UnparsedTokens,
                            ErrorMessageList.Empty.With(ErrorMessage.Expected(
                                string.Format("{0} occurring no {1} than exactly {2} times", _item, times > _n ? "more" : "less", _n))
                        ));
                    break;
                case QuantificationRule.NtoM:
                    if (times < _n)
                        return new Error<IList<TItem>>(
                            reply.UnparsedTokens,
                            ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occurring no less than between {_n} and {_m} times"))
                        );
                    break;
                case QuantificationRule.NOrLess:
                    if (times > _n)
                        return new Error<IList<TItem>>(
                            reply.UnparsedTokens,
                            ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occurring no more than {_n} times"))
                        );
                    break;
            }

            return new Parsed<IList<TItem>>(list, reply.UnparsedTokens, reply.ErrorMessages);
        }

        public override string ToString()
        {
            switch (_quantificationRule)
            {
                case QuantificationRule.NtoM:
                    return $"<[{_n} TO {_m} TIMES {_item.Name}>";
                case QuantificationRule.ExactlyN:
                    return $"<[{_n} TIMES {_item.Name}>";
            }

            return $"<{_n}+ TIMES {_item.Name}>";
        }

        public string Name => ToString();
    }
}
