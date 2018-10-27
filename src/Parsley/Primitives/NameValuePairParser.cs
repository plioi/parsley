using System;
using System.Collections.Generic;

namespace Parsley.Primitives
{
    public class NameValuePairParser<TName, TDelimiter, TValue> : IParser<KeyValuePair<TName, TValue>>
    {
        public NameValuePairParser(IParser<TName> name, IParser<TDelimiter> delimiter, IParser<TValue> value)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _delimiter = delimiter ?? throw new ArgumentNullException(nameof(delimiter));
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Name => $"<N {_name.Name} D {_delimiter.Name} V {_value.Name}>";

        public Reply<KeyValuePair<TName, TValue>> Parse(TokenStream tokens)
        {
            var name = _name.Parse(tokens);

            if (!name.Success)
                return Error<KeyValuePair<TName, TValue>>.From(name);

            var delimiter = _delimiter.Parse(name.UnparsedTokens);

            if (!delimiter.Success)
                return Error<KeyValuePair<TName, TValue>>.From(delimiter);

            var value = _value.Parse(delimiter.UnparsedTokens);

            if (!value.Success)
                return Error<KeyValuePair<TName, TValue>>.From(value);

            return new Parsed<KeyValuePair<TName, TValue>>(new KeyValuePair<TName, TValue>(name.Value, value.Value), value.UnparsedTokens, value.ErrorMessages);
        }

        private readonly IParser<TName> _name;
        private readonly IParser<TDelimiter> _delimiter;
        private readonly IParser<TValue> _value;
    }
}
