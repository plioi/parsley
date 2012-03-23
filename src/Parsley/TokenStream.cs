using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parsley
{
    public class TokenStream : IEnumerable<Token>
    {
        public static readonly TokenKind EndOfInput = new Pattern("end of input", @"$");
        public static readonly TokenKind Unknown = new Pattern("Unknown", @".*");

        private readonly Text text;
        private readonly List<TokenKind> kinds;

        private readonly Token current;
        private readonly Lazy<TokenStream> lazyAdvance;

        public TokenStream(Text text, params TokenKind[] kinds)
            : this(text, kinds.Concat(new[] { EndOfInput, Unknown }).ToList()) { }

        private TokenStream(Text text, List<TokenKind> kinds)
        {
            current = GetToken(text, kinds);

            //After exiting this loop, Current will be the
            //next unskippable token, and text will indicate
            //where that token starts.
            while (current.Kind.Skippable)
            {
                text = text.Advance(current.Literal.Length);

                current = GetToken(text, kinds);
            }

            this.text = text;
            this.kinds = kinds;

            lazyAdvance = new Lazy<TokenStream>(LazyAdvance);
        }

        private static Token GetToken(Text text, IEnumerable<TokenKind> kinds)
        {
            Token token;
            foreach (var kind in kinds)
                if (kind.TryMatch(text, out token))
                    return token;

            return null; //EndOfInput and Unknown guarantee this is unreachable.
        }

        private TokenStream LazyAdvance()
        {
            if (text.EndOfInput)
                return this;

            return new TokenStream(text.Advance(Current.Literal.Length), kinds);
        }

        public Token Current
        {
            get { return current; }
        }

        public TokenStream Advance()
        {
            return lazyAdvance.Value;
        }

        public Position Position
        {
            get { return Current.Position; }
        }

        public IEnumerator<Token> GetEnumerator()
        {
            var current = Current;

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