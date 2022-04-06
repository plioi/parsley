namespace Parsley
{
    public interface IParser<out T>
    {
        Reply<T> Parse(TokenStream tokens);
    }
}