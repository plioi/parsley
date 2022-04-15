namespace Parsley;

public class Error<T> : Reply<T>
{
    public Error(ErrorMessage error)
        : this(ErrorMessageList.Empty.With(error)) { }

    public Error(ErrorMessageList errors)
        => ErrorMessages = errors;

    public T Value => throw new MemberAccessException($"{ErrorMessages}");
    public bool Success => false;
    public ErrorMessageList ErrorMessages { get; }
}
