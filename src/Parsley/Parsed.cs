namespace Parsley;

public class Parsed<T> : Reply<T>
{
    public Parsed(T value, Position position, bool endOfInput)
        :this(value, position, endOfInput, ErrorMessageList.Empty) { }

    public Parsed(T value, Position position, bool endOfInput, ErrorMessageList potentialErrors)
    {
        Value = value;
        Position = position;
        EndOfInput = endOfInput;
        ErrorMessages = potentialErrors;
    }

    public T Value { get; }
    public Position Position { get; }
    public bool EndOfInput { get; }
    public bool Success => true;
    public ErrorMessageList ErrorMessages { get; }
}
