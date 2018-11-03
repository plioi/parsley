namespace Parsley
{
    public interface IParserG : INamed
    {
        IReplyG ParseG(TokenStream tokens);
    }
}
