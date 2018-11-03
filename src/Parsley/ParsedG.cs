namespace Parsley
{
    public class ParsedG : IReplyG
    {
        public ParsedG(TokenStream unparsedTokens)
        {
            UnparsedTokens = unparsedTokens;
        }
        
        public TokenStream UnparsedTokens { get; }
        public bool Success => true;

        public virtual ErrorMessageList ErrorMessages => ErrorMessageList.Empty;
    }
}
