namespace Parsley
{
    public interface Parser<out T>
    {
        Reply<T> Parse(Lexer tokens);
    }
}