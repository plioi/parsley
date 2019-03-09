using System;
using System.Linq;
using System.Text;

namespace Parsley.Parsers
{
    public class ChoiceParser<T> : Parser<T>
    {
        public ChoiceParser(IParser<T>[] parsers)
        {
            if (parsers == null)
                throw new ArgumentNullException(nameof(parsers));

            if (parsers.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(parsers), "There should be at least one parser.");

            _parsers = parsers;
        }

        public override IReply<T> Parse(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _parsers[0].Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            var errors = ErrorMessageList.Empty;
            var i = 1;

            while (!reply.Success && oldPosition == newPosition && i < _parsers.Length)
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

        public override IReplyG ParseG(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _parsers[0].ParseG(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            var errors = ErrorMessageList.Empty;
            var i = 1;

            while (!reply.Success && oldPosition == newPosition && i < _parsers.Length)
            {
                errors = errors.Merge(reply.ErrorMessages);
                reply = _parsers[i].ParseG(tokens);
                newPosition = reply.UnparsedTokens.Position;
                i++;
            }

            if (oldPosition == newPosition)
            {
                if (reply.Success)
                    return new ParsedG(reply.UnparsedTokens);

                return new Error<T>(reply.UnparsedTokens, errors.Merge(reply.ErrorMessages));
            }

            return reply;
        }

        protected override string GetName()
        {
            var sb = new StringBuilder("<CHOICE ");

            sb.Append(string.Join(" OR ", _parsers.Select(p => p.Name)));
            sb.Append(">");

            return sb.ToString();
        }

        private readonly IParser<T>[] _parsers;
    }
}