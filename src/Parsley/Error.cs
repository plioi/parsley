namespace Parsley;

public class Error<T> : Reply<T>
{
    public Error(Position position, bool endOfInput, ErrorMessage error)
        : this(position, endOfInput, ErrorMessageList.Empty.With(error)) { }

    public Error(Position position, bool endOfInput, ErrorMessageList errors)
    {
        Position = position;
        EndOfInput = endOfInput;
        ErrorMessages = errors;
    }

    public T Value => throw new MemberAccessException($"{Position}: {ErrorMessages}");
    public Position Position { get; }
    public bool EndOfInput { get; }
    public bool Success => false;
    public ErrorMessageList ErrorMessages { get; }
}
