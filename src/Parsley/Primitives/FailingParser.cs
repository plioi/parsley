namespace Parsley.Primitives
{
    public class FailingParser<T> : IParser<T>
    {
        public Reply<T> Parse(TokenStream tokens)
            => new Error<T>(tokens, ErrorMessage.Unknown());

        public override string ToString()
        {
            return "<fail>";
        }
    }
}