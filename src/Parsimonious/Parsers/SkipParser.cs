using System;
using System.Text;

namespace Parsimonious.Parsers
{
    public class SkipParser : Parser
    {
        public SkipParser(params IParserG[] parsers)
        {
            if (parsers == null)
                throw new ArgumentNullException(nameof(parsers));

            if (parsers.Length < 1)
                throw new ArgumentOutOfRangeException(nameof(parsers), "There should be at least one parser.");

            for (int i = 0; i < parsers.Length; ++i)
                if (parsers[i] == null)
                    throw new ArgumentNullException($"{nameof(parsers)}[{i}]");

            _parsers = parsers;
        }

        public override IReplyG ParseG(TokenStream tokens)
        {
            IReplyG reply = null;

            var unparsedTokens = tokens;

            foreach (var parser in _parsers)
            {
                reply = parser.ParseG(unparsedTokens);

                if (!reply.Success)
                    return reply;

                unparsedTokens = reply.UnparsedTokens;
            }

            return reply;
        }

        protected override string GetName()
        {
            var sb = new StringBuilder("<SKIP ");

            sb.Append(string.Join<IParserG>(" ", _parsers));
            sb.Append(">");

            return sb.ToString();
        }

        private readonly IParserG[] _parsers;
    }
}
