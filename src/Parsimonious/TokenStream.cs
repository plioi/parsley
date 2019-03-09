namespace Parsimonious
{
    using System;
    using System.Collections.Generic;

    public class TokenStream
    {
        private readonly Lazy<TokenStream> _rest;

        public TokenStream(IEnumerable<Token> tokens)
        {
            var enumerator = tokens.GetEnumerator();

            Current = enumerator.MoveNext()
                          ? enumerator.Current
                          : new Token(TokenKind.EndOfInput, new Position(1, 1), "");

            _rest = new Lazy<TokenStream>(() => LazyAdvance(enumerator));
        }

        private TokenStream(Token current, IEnumerator<Token> enumerator)
        {
            Current = current;
            _rest = new Lazy<TokenStream>(() => LazyAdvance(enumerator));
        }

        private TokenStream(Token current)
        {
            Current = current;
            _rest = new Lazy<TokenStream>(() => this);
        }

        public Token Current { get; }

        public TokenStream Advance()
        {
            return _rest.Value;
        }

        public Position Position => Current.Position;

        public override string ToString() => $">{Current}";

        private TokenStream LazyAdvance(IEnumerator<Token> enumerator)
        {
            if (enumerator.MoveNext())
                return new TokenStream(enumerator.Current, enumerator);

            if (Current.Kind == TokenKind.EndOfInput)
                return this;

            var endPosition = new Position(Position.Line, Position.Column + Current.Literal.Length);

            return new TokenStream(new Token(TokenKind.EndOfInput, endPosition, ""));
        }
    }
}
