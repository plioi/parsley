namespace Parsley;

public class Error<T> : Reply<T>
{
    public Error(Text unparsedInput, Position position, bool endOfInput, ErrorMessage error)
        : this(unparsedInput, position, endOfInput, ErrorMessageList.Empty.With(error)) { }

    public Error(Text unparsedInput, Position position, bool endOfInput, ErrorMessageList errors)
    {
        UnparsedInput = unparsedInput;
        Position = position;
        EndOfInput = endOfInput;
        ErrorMessages = errors;
    }

    public T Value => throw new MemberAccessException($"{Position}: {ErrorMessages}");
    public Text UnparsedInput { get; }
    public Position Position { get; }
    public bool EndOfInput { get; }
    public bool Success => false;
    public ErrorMessageList ErrorMessages { get; }
}
