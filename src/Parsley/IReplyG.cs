namespace Parsley
{
    public interface IReplyG
    {
        TokenStream UnparsedTokens { get; }
        bool Success { get; }
        ErrorMessageList ErrorMessages { get; }
    }
}
