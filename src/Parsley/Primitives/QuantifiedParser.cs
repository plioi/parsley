using System;
using System.Collections.Generic;

namespace Parsley.Primitives
{
    public enum QuantificationRule
    {
        AtLeastNTimes,
        ExactlyNTimes,
        FromNtoMTimes,
        NoMoreThanNTimes
    }

    public class QuantifiedParser<TItem, TSeparator> : IParser<IEnumerable<TItem>>
    {
        private readonly IParser<TItem> _item;
        private readonly QuantificationRule _quantificationRule;
        private readonly int _nTimes;
        private readonly int _mTimes;
        private readonly IParser<TSeparator> _itemSeparator;
        
        public QuantifiedParser(IParser<TItem> item, QuantificationRule quantificationRule, int nTimes, int mTimes = -1, IParser<TSeparator> itemSeparator = null)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));

            if (nTimes < 0)
                throw new ArgumentOutOfRangeException(nameof(nTimes), "should be non-negative");

            switch (quantificationRule)
            {
                case QuantificationRule.ExactlyNTimes:
                case QuantificationRule.AtLeastNTimes:
                    if (mTimes != -1)
                        throw new ArgumentOutOfRangeException(nameof(mTimes), "this value is not used in this mode and should be left -1");
                    break;
                case QuantificationRule.FromNtoMTimes:
                    if (nTimes > mTimes)
                        throw new ArgumentOutOfRangeException(nameof(mTimes), "should not be less than nTimes");
                    break;
            }

            if (item == itemSeparator)
                throw new ArgumentException("parser for the item and the separator cannot be the same one", nameof(itemSeparator));

            _quantificationRule = quantificationRule;

            _nTimes = nTimes;
            _mTimes = mTimes;

            _itemSeparator = itemSeparator;
        }

        public Reply<IEnumerable<TItem>> Parse(TokenStream tokens)
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
                    case QuantificationRule.ExactlyNTimes:
                        if (times > _nTimes)
                            return new Error<IEnumerable<TItem>>(
                                reply.UnparsedTokens,
                                ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occurring no more than exactly {_nTimes} times"))
                            );
                        break;
                    case QuantificationRule.FromNtoMTimes:
                        if (times > _mTimes)
                            return new Error<IEnumerable<TItem>>(
                                reply.UnparsedTokens,
                                ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occurring no more than between {_nTimes} and {_mTimes} times"))
                            );
                        break;
                    case QuantificationRule.NoMoreThanNTimes:
                        if (times > _nTimes)
                            return new Error<IEnumerable<TItem>>(
                                reply.UnparsedTokens,
                                ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occurring no more than {_nTimes} times"))
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
                    return new Error<IEnumerable<TItem>>(reply.UnparsedTokens, reply.ErrorMessages);

                newPosition = reply.UnparsedTokens.Position;
            }

            //The item parser finally failed or the separator parser parsed the next separator, but there was no item following it
            if (oldPosition != newPosition || separatorParserIsPresent && separatorWasParsed)
                return new Error<IEnumerable<TItem>>(reply.UnparsedTokens, reply.ErrorMessages);

            switch (_quantificationRule)
            {
                case QuantificationRule.AtLeastNTimes:
                    if (times < _nTimes)
                        return new Error<IEnumerable<TItem>>(
                            reply.UnparsedTokens,
                            ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occurring {_nTimes}+ times"))
                        );
                    break;
                case QuantificationRule.ExactlyNTimes:
                    if (times != _nTimes)
                        return new Error<IEnumerable<TItem>>(
                            reply.UnparsedTokens,
                            ErrorMessageList.Empty.With(ErrorMessage.Expected(
                                string.Format("{0} occurring no {1} than exactly {2} times", _item, times > _nTimes ? "more" : "less", _nTimes))
                        ));
                    break;
                case QuantificationRule.FromNtoMTimes:
                    if (times < _nTimes)
                        return new Error<IEnumerable<TItem>>(
                            reply.UnparsedTokens,
                            ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occurring no less than between {_nTimes} and {_mTimes} times"))
                        );
                    break;
                case QuantificationRule.NoMoreThanNTimes:
                    if (times > _nTimes)
                        return new Error<IEnumerable<TItem>>(
                            reply.UnparsedTokens,
                            ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occurring no more than {_nTimes} times"))
                        );
                    break;
            }

            return new Parsed<IEnumerable<TItem>>(list, reply.UnparsedTokens, reply.ErrorMessages);
        }

        public override string ToString()
        {
            switch (_quantificationRule)
            {
                case QuantificationRule.FromNtoMTimes:
                    return $"<[{_nTimes} TO {_mTimes} TIMES {_item}>";
                case QuantificationRule.ExactlyNTimes:
                    return $"<[{_nTimes} TIMES {_item}>";
            }

            return $"<{_nTimes}+ TIMES {_item}>";
        }
    }
}
