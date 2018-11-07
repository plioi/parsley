namespace Parsley
{
    /// <summary>
    /// General non-value-specific parser.
    /// </summary>
    public interface IParserG : INamed
    {
        IReplyG ParseG(TokenStream tokens);
    }
}
