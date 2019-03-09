using Parsimonious.Parsers;
using System;

namespace Parsimonious
{
    public class GrammarRule<T> : Parser<T>, INamedInternal
    {
        private IParser<T> _parser;

        public GrammarRule(string name = null)
        {
            _name = name;
        }

        protected override string GetName() => _name ?? _parser?.Name;

        void INamedInternal.SetName(string name)
        {
            _name = name;
        }

        private string _name;

        public IParser<T> Rule
        {
            get => _parser;

            set
            {
                if (_parser != null)
                    throw new InvalidOperationException($"Rule {Name} is already initialized with {_parser.Name}.");

                _parser = value ?? throw new ArgumentNullException(nameof(value));

                if (Name == null)
                    _name = _parser.Name;
            }
        }

        public override IReply<T> Parse(TokenStream tokens)
        {
            if (_parser == null)
                throw new InvalidOperationException($"Rule {Name} is not initialized.");

            return _parser.Parse(tokens);
        }
    }
}