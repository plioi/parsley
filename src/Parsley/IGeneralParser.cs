namespace Parsley
{
    public interface IGeneralParser : INamed
    {
        IGeneralReply ParseGeneral(TokenStream tokens);
    }
}
