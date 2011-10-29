namespace Parsley
{
    public interface Reply<out T>
    {
        T Value { get; }
        Lexer UnparsedTokens { get; }
        bool Success { get; }
        ErrorMessageList ErrorMessages { get; }
    }
}