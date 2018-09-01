namespace Parsley
{
    using System;

    public abstract class GrammarRule
    {
        public string Name { get; internal set; }

        protected GrammarRule(string name = null)
        {
            Name = name;
        }
    }

    public class GrammarRule<T> : GrammarRule, IParser<T>
    {
        private Func<TokenStream, Reply<T>> _parse;

        public GrammarRule(string name = null)
            : base(name)
        {
            _parse = tokens => new Error<T>(tokens, new UndefinedGrammarRuleErrorMessage(Name));
        }

        public IParser<T> Rule
        {
            set => _parse = value.Parse;
        }

        public Reply<T> Parse(TokenStream tokens)
        {
            return _parse(tokens);
        }
    }
}