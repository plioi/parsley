using System;

namespace Parsley
{
    public class LambdaParser<T> : Parser<T>
    {
        private Func<Lexer, Reply<T>> parse { get; set; }

        public LambdaParser(Func<Lexer, Reply<T>> parse)
        {
            this.parse = parse;
        }

        public Reply<T> Parse(Lexer tokens)
        {
            return parse(tokens);
        }
    }
}