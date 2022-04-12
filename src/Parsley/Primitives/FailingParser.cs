namespace Parsley;

partial class Grammar
{
    public static Parser<T> Fail<T>()
    {
        return input => new Error<T>(input, ErrorMessage.Unknown());
    }
}
