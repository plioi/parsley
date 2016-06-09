namespace Parsley
{
    using System;
    using System.Collections.Generic;

    public class TokenStream
    {
        private readonly Token current;
        private readonly Lazy<TokenStream> rest;

        public TokenStream(IEnumerable<Token> tokens)
        {
            var enumerator = tokens.GetEnumerator();

            current = enumerator.MoveNext()
                          ? enumerator.Current
                          : new Token(TokenKind.EndOfInput, new Position(1, 1), "");

            rest = new Lazy<TokenStream>(() => LazyAdvance(enumerator));
        }

        private TokenStream(Token current, IEnumerator<Token> enumerator)
        {
            this.current = current;
            rest = new Lazy<TokenStream>(() => LazyAdvance(enumerator));
        }

        private TokenStream(Token current)
        {
            this.current = current;
            rest = new Lazy<TokenStream>(() => this);
        }

        public Token Current
        {
            get { return current; }
        }

        public TokenStream Advance()
        {
            return rest.Value;
        }

        public Position Position
        {
            get { return Current.Position; }
        }

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
