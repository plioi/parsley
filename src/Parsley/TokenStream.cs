using System;
using System.Collections;
using System.Collections.Generic;

namespace Parsley
{
    public class TokenStream : IEnumerable<Token>
    {
        private readonly Token current;
        private readonly Lazy<TokenStream> rest;

        public TokenStream(IEnumerator<Token> enumerator)
        {
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

        public IEnumerator<Token> GetEnumerator()
        {
            var head = Current;

            yield return head;

            if (head.Kind != TokenKind.EndOfInput)
                foreach (var token in Advance())
                    yield return token;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
