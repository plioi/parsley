namespace Parsley
{
    public class ErrorGeneral : IGeneralReply
    {
        public ErrorGeneral(TokenStream unparsedTokens, ErrorMessage error)
            : this(unparsedTokens, ErrorMessageList.Empty.With(error)) { }

        public ErrorGeneral(TokenStream unparsedTokens, ErrorMessageList errors)
        {
            UnparsedTokens = unparsedTokens;
            ErrorMessages = errors;
        }

        public static ErrorGeneral From(IGeneralReply r)
        {
            return new ErrorGeneral(r.UnparsedTokens, r.ErrorMessages);
        }

        public TokenStream UnparsedTokens { get; }
        public bool Success => false;
        public ErrorMessageList ErrorMessages { get; }
    }
}
