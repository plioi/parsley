namespace Parsley
{
    public interface Reply<out T>
    {
        T Value { get; }
        TokenStream UnparsedTokens { get; }
        bool Success { get; }
        ErrorMessageList ErrorMessages { get; }
    }
}