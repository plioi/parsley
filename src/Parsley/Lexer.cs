using System;
using System.Collections.Generic;

namespace Parsley
{
    public class Lexer
    {
        private readonly TokenKind[] _kinds;

        public Lexer(params TokenKind[] kinds)
        {
            _kinds = kinds ?? throw new ArgumentNullException(nameof(kinds));
        }

        public virtual IEnumerable<Token> Tokenize(string input)
        {
            var text = new Text(input);

            while (!text.EndOfInput)
            {
                var current = GetToken(text);

                if (current == null)
                {
                    TokenKind.Unknown.TryMatch(text, out Token unknown);

                    if (unknown == null)
                        throw new InvalidOperationException("unknown token failed to match non-empty text");

                    yield return unknown;
                    yield break;
                }

                text.Advance(current.Literal.Length);

                if (current.Kind.Skippable)
                    continue;

                yield return current;
            }
        }

        private Token GetToken(Text text)
        {
            foreach (var kind in _kinds)
                if (kind.TryMatch(text, out Token token))
                    return token;

            return null;
        }
    }
}