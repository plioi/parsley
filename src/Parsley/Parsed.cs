namespace Parsley;

public class Parsed<T> : Reply<T>
{
    public Parsed(T value, Input unparsedInput)
        :this(value, unparsedInput, ErrorMessageList.Empty) { }

    public Parsed(T value, Input unparsedInput, ErrorMessageList potentialErrors)
    {
        Value = value;
        UnparsedInput = unparsedInput;
        ErrorMessages = potentialErrors;
    }

    public T Value { get; }
    public Input UnparsedInput { get; }
    public bool Success => true;
    public ErrorMessageList ErrorMessages { get; }
}
