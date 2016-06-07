namespace Parsley
{
    using System;

    public class GrammarRule<T> : Parser<T>
    {
        private Func<TokenStream, Reply<T>> parse;

        public GrammarRule(string name = null)
        {
            Name = name;
            parse = tokens => new Error<T>(tokens, new UndefinedGrammarRuleErrorMessage(Name));
        }

        public string Name { get; internal set; }

        public Parser<T> Rule
        {
            set { parse = value.Parse; }
        }

        public Reply<T> Parse(TokenStream tokens)
        {
            return parse(tokens);
        }
    }
}