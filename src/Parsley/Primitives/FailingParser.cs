namespace Parsley.Primitives;

class FailingParser<T> : IParser<T>
{
    public Reply<T> Parse(Input input)
        => new Error<T>(input, ErrorMessage.Unknown());
}
