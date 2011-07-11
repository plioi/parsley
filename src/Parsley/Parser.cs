namespace Parsley
{
    public delegate Reply<T> Parser<out T>(Lexer tokens);
}