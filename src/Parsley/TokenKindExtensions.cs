using System;
using Parsley.Parsers;

namespace Parsley
{
    public static class TokenKindExtensions
    {
        public static IParserG Kind(this TokenKind tokenKind)
            => new TokenByKindParser(tokenKind);

        public static IParser<string> Literal(this TokenKind tokenKind)
            => new ReturnTokenLiteralParser(tokenKind);

        public static IParser<TResult> Literal<TResult>(this TokenKind kind, Func<string, TResult> resultContinuation)
            => new MapTokenLiteralParser<TResult>(kind, resultContinuation);
    }
}
