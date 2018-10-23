using System;
using System.Collections.Generic;

namespace Parsley.Primitives
{
    internal class ZeroOrMoreParser<T> : IParser<IEnumerable<T>>
    {
        private readonly IParser<T> _item;

        public ZeroOrMoreParser(IParser<T> item)
        {
            _item = item;
        }

        public Reply<IEnumerable<T>> Parse(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _item.Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            var list = new List<T>();

            while (reply.Success)
            {
                if (oldPosition == newPosition)
                    throw new Exception($"Parser encountered a potential infinite loop at position {newPosition}.");

                list.Add(reply.Value);

                oldPosition = newPosition;
                reply = _item.Parse(reply.UnparsedTokens);
                newPosition = reply.UnparsedTokens.Position;
            }

            //The item parser finally failed.

            if (oldPosition != newPosition)
                return new Error<IEnumerable<T>>(reply.UnparsedTokens, reply.ErrorMessages);

            return new Parsed<IEnumerable<T>>(list, reply.UnparsedTokens, reply.ErrorMessages);
        }

        public override string ToString() => $"<0+ {_item}>";
    }
}