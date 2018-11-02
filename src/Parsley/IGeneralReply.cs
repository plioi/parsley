namespace Parsley
{
    public interface IGeneralReply
    {
        TokenStream UnparsedTokens { get; }
        bool Success { get; }
        ErrorMessageList ErrorMessages { get; }
    }
}
