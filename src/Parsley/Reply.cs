namespace Parsley;

public interface Reply<out T>
{
    T Value { get; }
    Input UnparsedTokens { get; }
    bool Success { get; }
    ErrorMessageList ErrorMessages { get; }
}
