namespace Parsley;

public partial class Grammar<T>
{
    public static readonly Parser<T> Fail = (ref Text input) => new Error<T>();
}
