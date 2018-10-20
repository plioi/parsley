﻿namespace Parsley
{
    public class MonadicUnitParser<T> : IParser<T>
    {
        public MonadicUnitParser(T value)
        {
            _value = value;
        }

        Reply<T> IParser<T>.Parse(TokenStream tokens) => new Parsed<T>(_value, tokens);

        private readonly T _value;

        public override string ToString()
        {
            return $"<unit {_value}>";
        }
    }
}