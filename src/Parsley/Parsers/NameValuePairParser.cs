using System;
using System.Collections.Generic;

namespace Parsley.Parsers
{
    public class NameValuePairParser<TName, TValue> : Parser<KeyValuePair<TName, TValue>>
    {
        public NameValuePairParser(IParser<TName> name, IParserG delimiter, IParser<TValue> value)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _delimiter = delimiter ?? throw new ArgumentNullException(nameof(delimiter));
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        protected override string GetName() => $"<N {_name.Name} D {_delimiter.Name} V {_value.Name}>";

        public override IReply<KeyValuePair<TName, TValue>> Parse(TokenStream tokens)
        {
            var name = _name.Parse(tokens);

            if (!name.Success)
                return Error<KeyValuePair<TName, TValue>>.From(name);

            var delimiter = _delimiter.ParseG(name.UnparsedTokens);

            if (!delimiter.Success)
                return Error<KeyValuePair<TName, TValue>>.From(delimiter);

            var value = _value.Parse(delimiter.UnparsedTokens);

            if (!value.Success)
                return Error<KeyValuePair<TName, TValue>>.From(value);

            return new Parsed<KeyValuePair<TName, TValue>>(new KeyValuePair<TName, TValue>(name.Value, value.Value), value.UnparsedTokens, value.ErrorMessages);
        }

        public override IReplyG ParseG(TokenStream tokens)
        {
            var name = _name.ParseG(tokens);

            if (!name.Success)
                return name;

            var delimiter = _delimiter.ParseG(name.UnparsedTokens);

            if (!delimiter.Success)
                return delimiter;

            return _value.Parse(delimiter.UnparsedTokens);
        }

        private readonly IParser<TName> _name;
        private readonly IParserG _delimiter;
        private readonly IParser<TValue> _value;
    }
}
