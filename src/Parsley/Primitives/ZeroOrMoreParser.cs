namespace Parsley.Primitives
{
    using System;
    using System.Collections.Generic;

    internal class ZeroOrMoreParser<T> : Parser<IEnumerable<T>>
    {
        private readonly Parser<T> item;

        public ZeroOrMoreParser(Parser<T> item)
        {
            this.item = item;
        }

        public Reply<IEnumerable<T>> Parse(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = item.Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            var list = new List<T>();

            while (reply.Success)
            {
                if (oldPosition == newPosition)
                    throw new Exception($"Parser encountered a potential infinite loop at position {newPosition}.");

                list.Add(reply.Value);
                oldPosition = newPosition;
                reply = item.Parse(reply.UnparsedTokens);
                newPosition = reply.UnparsedTokens.Position;
            }

            //The item parser finally failed.

            if (oldPosition != newPosition)
                return new Error<IEnumerable<T>>(reply.UnparsedTokens, reply.ErrorMessages);

            return new Parsed<IEnumerable<T>>(list, reply.UnparsedTokens, reply.ErrorMessages);
        }
    }
}