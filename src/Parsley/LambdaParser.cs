using System;

namespace Parsley
{
    public class LambdaParser<T> : IParser<T>
    {
        private readonly Func<TokenStream, Reply<T>> _parse;

        public LambdaParser(Func<TokenStream, Reply<T>> parse)
        {
            _parse = parse;
        }

        public Reply<T> Parse(TokenStream tokens)
        {
            return _parse(tokens);
        }
    }
}