namespace Parsley;

public class Parsed<T> : Reply<T>
{
    public Parsed(T value)
    {
        Value = value;
    }

    public T Value { get; }
    public bool Success => true;
    public ErrorMessageList ErrorMessages => ErrorMessageList.Empty;
}
