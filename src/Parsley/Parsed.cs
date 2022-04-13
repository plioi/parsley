namespace Parsley;

public class Parsed<T> : Reply<T>
{
    public Parsed(T value, Position position)
        :this(value, position, ErrorMessageList.Empty) { }

    public Parsed(T value, Position position, ErrorMessageList potentialErrors)
    {
        Value = value;
        Position = position;
        ErrorMessages = potentialErrors;
    }

    public T Value { get; }
    public Position Position { get; }
    public bool Success => true;
    public ErrorMessageList ErrorMessages { get; }
}
