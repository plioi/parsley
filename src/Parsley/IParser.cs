namespace Parsley
{
    public interface IParser<out T> : INamed
    {
        Reply<T> Parse(TokenStream tokens);
    }
}