namespace Parsley;

public class Error<T> : Reply<T>
{
    public Error(Input unparsedInput, ErrorMessage error)
        : this(unparsedInput,  ErrorMessageList.Empty.With(error)) { }

    public Error(Input unparsedInput, ErrorMessageList errors)
    {
        UnparsedInput = unparsedInput;
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

    public Input UnparsedInput { get; }
    public bool Success => false;
    public ErrorMessageList ErrorMessages { get; }
}
