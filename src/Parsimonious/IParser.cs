namespace Parsimonious
{
    public interface IParser<out T> : IParserG
    {
        IReply<T> Parse(TokenStream tokens);
    }
}