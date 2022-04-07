namespace Parsley;

public class Parsed<T> : Reply<T>
{
    public Parsed(T value, Input unparsedTokens)
        :this(value, unparsedTokens, ErrorMessageList.Empty) { }

    public Parsed(T value, Input unparsedTokens, ErrorMessageList potentialErrors)
    {
        Value = value;
        UnparsedTokens = unparsedTokens;
        ErrorMessages = potentialErrors;
    }

    public T Value { get; }
    public Input UnparsedTokens { get; }
    public bool Success => true;
    public ErrorMessageList ErrorMessages { get; }
}
