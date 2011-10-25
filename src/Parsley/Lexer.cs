using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parsley
{
    public class Lexer : IEnumerable<Token>
    {
        public static readonly TokenKind EndOfInput = new TokenKind("end of input", @"$");
        public static readonly TokenKind Unknown = new TokenKind("Unknown", @".*");

        private readonly Text text;
        private readonly List<TokenKind> kinds;

        private readonly Token currentToken;
        private readonly Lazy<Lexer> lazyAdvance;

        public Lexer(Text text, params TokenKind[] kinds)
            : this(text, kinds.Concat(new[] { EndOfInput, Unknown }).ToList()) { }

        private Lexer(Text text, List<TokenKind> kinds)
        {
            currentToken = GetToken(text, kinds);

            //After exiting this loop, currentToken will be the
            //next unskippable token, and text will indicate
            //where that token starts.
            while (currentToken.Kind.Skippable)
            {
                text = text.Advance(currentToken.Literal.Length);

                currentToken = GetToken(text, kinds);
            }

            this.text = text;
            this.kinds = kinds;

            lazyAdvance = new Lazy<Lexer>(LazyAdvance);
        }

        private static Token GetToken(Text text, IEnumerable<TokenKind> kinds)
        {
            Token token;
            foreach (var kind in kinds)
                if (kind.TryMatch(text, out token))
                    return token;

            return null; //EndOfInput and Unknown guarantee this is unreachable.
        }

        private Lexer LazyAdvance()
        {
            if (text.EndOfInput)
                return this;

            return new Lexer(text.Advance(CurrentToken.Literal.Length), kinds);
        }

        public Token CurrentToken
        {
            get { return currentToken; }
        }

        public Lexer Advance()
        {
            return lazyAdvance.Value;
        }

        public Position Position
        {
            get { return text.Position; }
        }

        public override string ToString()
        {
            return text.ToString();
        }

        public IEnumerator<Token> GetEnumerator()
        {
            var current = CurrentToken;

            yield return current;

            if (current.Kind != EndOfInput)
                foreach (var token in Advance())
                    yield return token;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}