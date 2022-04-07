namespace Parsley.Primitives;

class FailingParser<T> : IParser<T>
{
    public Reply<T> Parse(TokenStream tokens)
        => new Error<T>(tokens, ErrorMessage.Unknown());
}
