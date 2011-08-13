using System;

namespace Parsley
{
    public interface Parser<out T>
    {
        Reply<T> Parse(Lexer lexer);
    }

    public class GrammarRule<T> : Parser<T>
    {
        private Func<Lexer, Reply<T>> ParseTokens { get; set; }

        public Parser<T> Rule
        {
            set { ParseTokens = value.Parse; }
        }

        public GrammarRule(Func<Lexer, Reply<T>> parse = null)
        {
            ParseTokens = parse;
        }

        public Reply<T> Parse(Lexer lexer)
        {
            return ParseTokens(lexer);
        }
    }
}