namespace Parsley.Primitives
{
    internal class FailingParser<T> : Parser<T>
    {
        public Reply<T> Parse(Lexer tokens)
        {
            return new Error<T>(tokens, ErrorMessage.Unknown());
        }
    }
}