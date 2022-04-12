namespace Parsley;

public partial class Grammar<T>
{
    public static readonly Parser<T> Fail = input => new Error<T>(input, input.Position, input.EndOfInput, ErrorMessage.Unknown());
}
