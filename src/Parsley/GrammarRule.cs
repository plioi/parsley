namespace Parsley
{
    using System;

    public abstract class GrammarRule
    {
        public string Name { get; internal set; }

        protected GrammarRule(string name = "")
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
            _parse = ParseNotInitialized;
        }

        public IParser<T> Rule
        {
            set
            {
                if (_parse != ParseNotInitialized)
                    throw new InvalidOperationException("Rule is already initialized.");

               _parse = value.Parse;
            }
        }

        public Reply<T> Parse(TokenStream tokens)
        {
            return _parse(tokens);
        }

        private Reply<T> ParseNotInitialized(TokenStream _)
            => throw new InvalidOperationException($"Rule {Name} is not initialized.");
    }
}