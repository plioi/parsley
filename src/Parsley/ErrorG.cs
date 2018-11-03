namespace Parsley
{
    public class ErrorG : IReplyG
    {
        public ErrorG(TokenStream unparsedTokens, ErrorMessage error)
            : this(unparsedTokens, ErrorMessageList.Empty.With(error)) { }

        public ErrorG(TokenStream unparsedTokens, ErrorMessageList errors)
        {
            UnparsedTokens = unparsedTokens;
            ErrorMessages = errors;
        }

        public static ErrorG From(IReplyG r)
        {
            return new ErrorG(r.UnparsedTokens, r.ErrorMessages);
        }

        public TokenStream UnparsedTokens { get; }
        public bool Success => false;
        public ErrorMessageList ErrorMessages { get; }
    }
}
