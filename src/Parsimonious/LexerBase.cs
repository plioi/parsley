using System;
using System.Collections.Generic;

namespace Parsimonious
{
    public abstract class LexerBase<TInput>
    {
        private readonly TokenKind[] _kinds;

        protected LexerBase(params TokenKind[] kinds)
        {
            _kinds = kinds ?? throw new ArgumentNullException(nameof(kinds));
        }

        public IReadOnlyList<TokenKind> TokenKinds => _kinds;

        public abstract IEnumerable<Token> Tokenize(TInput input);

        protected virtual Token GetToken(IText text)
        {
            foreach (var kind in _kinds)
                if (kind.TryMatch(text, out Token token))
                    return token;

            return null;
        }

        protected virtual Token MatchUnknownToken(IText text)
        {
            TokenKind.Unknown.TryMatch(text, out Token unknown);

            if (unknown == null)
                throw new InvalidOperationException("unknown token failed to match non-empty text");

            return unknown;
        }
    }
}
