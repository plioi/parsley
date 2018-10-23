using System;
using System.Collections.Generic;

namespace Parsley.Primitives
{
    public class QuantifiedParser<T> : IParser<IEnumerable<T>>
    {
        private readonly IParser<T> _item;
        private readonly Rule _rule;
        private readonly int _nTimes;
        private readonly int _mTimes;

        public enum Rule
        {
            AtLeastNTimes,
            ExactlyNTimes,
            FromNtoMTimes,
            NoMoreThanNTimes
        }

        public QuantifiedParser(IParser<T> item, Rule rule, int nTimes, int mTimes = -1)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));

            if (nTimes < 0)
                throw new ArgumentOutOfRangeException(nameof(nTimes), "should be non-negative");

            switch (rule)
            {
                case Rule.ExactlyNTimes:
                case Rule.AtLeastNTimes:
                    if (mTimes != -1)
                        throw new ArgumentOutOfRangeException(nameof(mTimes), "this value is not used in this mode and should be left -1");
                    break;
                case Rule.FromNtoMTimes:
                    if (nTimes > mTimes)
                        throw new ArgumentOutOfRangeException(nameof(mTimes), "should not be less than nTimes");
                    break;
            }

            _rule = rule;

            _nTimes = nTimes;
            _mTimes = mTimes;
        }

        public Reply<IEnumerable<T>> Parse(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _item.Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            var times = 0;

            var list = new List<T>();

            while (reply.Success)
            {
                if (oldPosition == newPosition)
                    throw new Exception($"Parser encountered a potential infinite loop at position {newPosition}.");

                ++times;

                switch (_rule)
                {
                    case Rule.ExactlyNTimes:
                        if (times > _nTimes)
                            return new Error<IEnumerable<T>>(
                                reply.UnparsedTokens,
                                ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occured more than exactly {_nTimes} times"))
                            );
                        break;
                    case Rule.FromNtoMTimes:
                        if (times > _mTimes)
                            return new Error<IEnumerable<T>>(
                                reply.UnparsedTokens,
                                ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occured more than between {_nTimes} and {_mTimes} times"))
                            );
                        break;
                    case Rule.NoMoreThanNTimes:
                        if (times > _nTimes)
                            return new Error<IEnumerable<T>>(
                                reply.UnparsedTokens,
                                ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occured more than {_nTimes} times"))
                            );
                        break;
                }

                list.Add(reply.Value);

                oldPosition = newPosition;
                reply = _item.Parse(reply.UnparsedTokens);
                newPosition = reply.UnparsedTokens.Position;
            }

            //The item parser finally failed.

            if (oldPosition != newPosition)
                return new Error<IEnumerable<T>>(reply.UnparsedTokens, reply.ErrorMessages);

            switch (_rule)
            {
                case Rule.AtLeastNTimes:
                    if (times < _nTimes)
                        return new Error<IEnumerable<T>>(
                            reply.UnparsedTokens,
                            ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occured less than at least {_nTimes} times"))
                        );
                    break;
                case Rule.ExactlyNTimes:
                    if (times != _nTimes)
                        return new Error<IEnumerable<T>>(
                            reply.UnparsedTokens,
                            ErrorMessageList.Empty.With(ErrorMessage.Expected(
                                string.Format("{0} occured {1} than exactly {2} times", _item, times > _nTimes ? "more" : "less", _nTimes))
                        ));
                    break;
                case Rule.FromNtoMTimes:
                    if (times < _nTimes)
                        return new Error<IEnumerable<T>>(
                            reply.UnparsedTokens,
                            ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occured less than between {_nTimes} and {_mTimes} times"))
                        );
                    break;
                case Rule.NoMoreThanNTimes:
                    if (times > _nTimes)
                        return new Error<IEnumerable<T>>(
                            reply.UnparsedTokens,
                            ErrorMessageList.Empty.With(ErrorMessage.Expected($"{_item} occured more than {_nTimes} times"))
                        );
                    break;
            }

            return new Parsed<IEnumerable<T>>(list, reply.UnparsedTokens, reply.ErrorMessages);
        }

        public override string ToString()
        {
            switch (_rule)
            {
                case Rule.FromNtoMTimes:
                    return $"<[{_nTimes} to {_mTimes} times {_item}>";
                case Rule.ExactlyNTimes:
                    return $"<[{_nTimes} times {_item}>";
            }

            return $"<{_nTimes}+ times {_item}>";
        }
    }
}
