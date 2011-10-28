using System;

namespace Parsley
{
    public class GrammarRule<T> : Parser<T>
    {
        private Func<Lexer, Reply<T>> parse;

        public GrammarRule(string name = null)
        {
            Name = name;
            parse = null;
        }

        public string Name { get; internal set; }

        public Parser<T> Rule
        {
            set { parse = value.Parse; }
        }

        public Reply<T> Parse(Lexer tokens)
        {
            return parse(tokens);
        }
    }
}