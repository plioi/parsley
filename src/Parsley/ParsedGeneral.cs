namespace Parsley
{
    public class ParsedGeneral : IGeneralReply
    {
        public ParsedGeneral(TokenStream unparsedTokens)
        {
            UnparsedTokens = unparsedTokens;
        }
        
        public TokenStream UnparsedTokens { get; }
        public bool Success => true;

        public virtual ErrorMessageList ErrorMessages => ErrorMessageList.Empty;
    }
}
