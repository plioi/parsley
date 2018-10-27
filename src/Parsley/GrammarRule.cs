namespace Parsley
{
    using System;

    public abstract class GrammarRule
    {
        public string Name { get; internal set; }

        protected GrammarRule(string name)
        {
            Name = name;
        }
    }

    public class GrammarRule<T> : GrammarRule, IParser<T>
    {
        private IParser<T> _parser;

        public GrammarRule(string name = null)
            : base(name)
        {
        }

        public IParser<T> Rule
        {
            get => _parser;

            set
            {
                if (_parser != null)
                    throw new InvalidOperationException($"Rule {Name} is already initialized with {_parser.Name}.");

                _parser = value ?? throw new ArgumentNullException(nameof(value));

                if (Name == null)
                    Name = _parser.Name;
            }
        }

        public Reply<T> Parse(TokenStream tokens)
        {
            if (_parser == null)
                throw new InvalidOperationException($"Rule {Name} is not initialized.");

            return _parser.Parse(tokens);
        }

        public override string ToString() => Name ?? _parser?.Name;
    }
}