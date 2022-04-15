namespace Parsley;

public class Error<T> : Reply<T>
{
    public Error(Position position, ErrorMessage error)
        : this(position, ErrorMessageList.Empty.With(error)) { }

    public Error(Position position, ErrorMessageList errors)
    {
        Position = position;
        ErrorMessages = errors;
    }

    public T Value => throw new MemberAccessException($"{ErrorMessages}");
    public Position Position { get; }
    public bool Success => false;
    public ErrorMessageList ErrorMessages { get; }
}
