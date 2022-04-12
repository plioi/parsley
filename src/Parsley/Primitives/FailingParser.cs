namespace Parsley.Primitives;

class FailingParser<T> : Parser<T>
{
    public Reply<T> Parse(Text input)
        => new Error<T>(input, ErrorMessage.Unknown());
}
