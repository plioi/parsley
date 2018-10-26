using System;
using System.Text;

namespace Parsley.Primitives
{
    internal class ChoiceParser<T> : IParser<T>
    {
        private readonly IParser<T>[] _parsers;

        public ChoiceParser(IParser<T>[] parsers)
        {
            if (parsers == null)
                throw new ArgumentNullException(nameof(parsers));

            if (parsers.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(parsers), "There should be at least one parser.");

            _parsers = parsers;
        }

        public Reply<T> Parse(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _parsers[0].Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            var errors = ErrorMessageList.Empty;
            var i = 1;
            while (!reply.Success && (oldPosition == newPosition) && i < _parsers.Length)
            {
                errors = errors.Merge(reply.ErrorMessages);
                reply = _parsers[i].Parse(tokens);
                newPosition = reply.UnparsedTokens.Position;
                i++;
            }
            if (oldPosition == newPosition)
            {
                errors = errors.Merge(reply.ErrorMessages);

                if (reply.Success)
                    return new Parsed<T>(reply.Value, reply.UnparsedTokens, errors);

                return new Error<T>(reply.UnparsedTokens, errors);
            }

            return reply;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("<choice ");

            sb.AppendJoin<IParser<T>>(" or ", _parsers);
            sb.Append(">");

            return sb.ToString();
        }
    }
}