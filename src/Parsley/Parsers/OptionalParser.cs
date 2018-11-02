using System;

namespace Parsley.Parsers
{
    public class OptionalParser<TItem> : IParser<TItem>
    {
        public OptionalParser(IParser<TItem> parser, TItem defaultValue = default(TItem))
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _defaultValue = defaultValue;
        }

        public Reply<TItem> Parse(TokenStream tokens)
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

        public override string ToString() => $"<OPTIONAL {_parser.Name} WITH DEFAULT {_defaultValue}>";
        public string Name => ToString();
        
        private readonly IParser<TItem> _parser;
        private readonly TItem _defaultValue;
    }
}
