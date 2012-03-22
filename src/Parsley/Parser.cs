namespace Parsley
{
    public interface Parser<out T>
    {
        Reply<T> Parse(TokenStream tokens);
    }
}