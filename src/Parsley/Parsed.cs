namespace Parsley;

public class Parsed<T> : Reply<T>
{
    public Parsed(T value, Text unparsedInput, Position position, bool endOfInput)
        :this(value, unparsedInput, position, endOfInput, ErrorMessageList.Empty) { }

    public Parsed(T value, Text unparsedInput, Position position, bool endOfInput, ErrorMessageList potentialErrors)
    {
        Value = value;
        UnparsedInput = unparsedInput;
        Position = position;
        EndOfInput = endOfInput;
        ErrorMessages = potentialErrors;
    }

    public T Value { get; }
    public Text UnparsedInput { get; }
    public Position Position { get; }
    public bool EndOfInput { get; }
    public bool Success => true;
    public ErrorMessageList ErrorMessages { get; }
}
