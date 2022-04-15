namespace Parsley;

public class Parsed<T> : Reply<T>
{
    public Parsed(T value)
        :this(value, ErrorMessageList.Empty) { }

    public Parsed(T value, ErrorMessageList potentialErrors)
    {
        Value = value;
        ErrorMessages = potentialErrors;
    }

    public T Value { get; }
    public bool Success => true;
    public ErrorMessageList ErrorMessages { get; }
}
