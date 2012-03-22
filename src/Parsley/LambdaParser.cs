using System;

namespace Parsley
{
    public class LambdaParser<T> : Parser<T>
    {
        private Func<TokenStream, Reply<T>> parse { get; set; }

        public LambdaParser(Func<TokenStream, Reply<T>> parse)
        {
            this.parse = parse;
        }

        public Reply<T> Parse(TokenStream tokens)
        {
            return parse(tokens);
        }
    }
}