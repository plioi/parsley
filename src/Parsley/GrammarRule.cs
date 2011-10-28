using System;

namespace Parsley
{
    public class GrammarRule<T> : Parser<T>
    {
        private Func<Lexer, Reply<T>> parse { get; set; }

        public Parser<T> Rule
        {
            set { parse = value.Parse; }
        }

        public GrammarRule()
        {
            parse = null;
        }

        public Reply<T> Parse(Lexer tokens)
        {
            return parse(tokens);
        }
    }
}