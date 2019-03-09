using System;

namespace Parsimonious.Parsers
{
    public class OptionalParser<TItem> : Parser<TItem>
    {
        public OptionalParser(IParser<TItem> parser, TItem defaultValue = default(TItem))
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _defaultValue = defaultValue;
        }

        public override IReply<TItem> Parse(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _parser.Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            if (reply.Success)
                return reply;

            if (oldPosition == newPosition)
                return new Parsed<TItem>(_defaultValue, reply.UnparsedTokens);

            return reply;
        }

        public override IReplyG ParseG(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _parser.ParseG(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            if (reply.Success || oldPosition == newPosition)
                return new ParsedG(reply.UnparsedTokens);

            return reply;
        }

        protected override string GetName() => $"<? {_parser.Name} ?? {_defaultValue}>";
        
        private readonly IParser<TItem> _parser;
        private readonly TItem _defaultValue;
    }
}
