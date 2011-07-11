using System;

namespace Parsley
{
    public interface Reply<out T>
    {
        T Value { get; }
        Lexer UnparsedTokens { get; }
        bool Success { get; }
        ErrorMessageList ErrorMessages { get; }
        Reply<U> ParseRest<U>(Func<T, Parser<U>> constructNextParser);
    }
}