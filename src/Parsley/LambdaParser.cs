namespace Parsley
{
    using System;

    public class LambdaParser<T> : Parser<T>
    {
        private readonly Func<TokenStream, Reply<T>> parse;

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