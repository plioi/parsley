namespace Parsley;

public class Error<T> : Reply<T>
{
    public Error(Text unparsedInput, ErrorMessage error)
        : this(unparsedInput,  ErrorMessageList.Empty.With(error)) { }

    public Error(Text unparsedInput, ErrorMessageList errors)
    {
        UnparsedInput = unparsedInput;
        Position = unparsedInput.Position;
        EndOfInput = unparsedInput.EndOfInput;
        ErrorMessages = errors;
    }

    public T Value
    {
        get
        {
            var position = UnparsedInput.Position;
            throw new MemberAccessException($"{position}: {ErrorMessages}");
        }
    }

    public Text UnparsedInput { get; }
    public Position Position { get; }
    public bool EndOfInput { get; }
    public bool Success => false;
    public ErrorMessageList ErrorMessages { get; }
}
