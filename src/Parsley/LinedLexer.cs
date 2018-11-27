using System;
using System.Collections.Generic;
using System.IO;

namespace Parsley
{
    public class LinedLexer
    {
        private readonly TokenKind[] _kinds;

        public LinedLexer(params TokenKind[] kinds)
        {
            _kinds = kinds ?? throw new ArgumentNullException(nameof(kinds));
        }

        public virtual IEnumerable<Token> Tokenize(TextReader textReader)
        {
            var text = new LinedText(textReader);

            while (text.ReadLine())
            {
                while (!text.EndOfLine)
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
        }

        private Token GetToken(LinedText text)
        {
            foreach (var kind in _kinds)
                if (kind.TryMatch(text, out Token token))
                    return token;

            return null;
        }
    }
}