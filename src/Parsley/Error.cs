namespace Parsley;

public class Error<T> : Reply<T>
{
    public Error(Input unparsedTokens, ErrorMessage error)
        : this(unparsedTokens,  ErrorMessageList.Empty.With(error)) { }

    public Error(Input unparsedTokens, ErrorMessageList errors)
    {
        UnparsedTokens = unparsedTokens;
        ErrorMessages = errors;
    }

    public T Value
    {
        get
        {
            var position = UnparsedTokens.Position;
            throw new MemberAccessException($"{position}: {ErrorMessages}");
        }
    }

    public Input UnparsedTokens { get; }
    public bool Success => false;
    public ErrorMessageList ErrorMessages { get; }
}
