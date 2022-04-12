namespace Parsley;

public class Parsed<T> : Reply<T>
{
    public Parsed(T value, Text unparsedInput)
        :this(value, unparsedInput, ErrorMessageList.Empty) { }

    public Parsed(T value, Text unparsedInput, ErrorMessageList potentialErrors)
    {
        Value = value;
        UnparsedInput = unparsedInput;
        Position = unparsedInput.Position;
        EndOfInput = unparsedInput.EndOfInput;
        ErrorMessages = potentialErrors;
    }

    public T Value { get; }
    public Text UnparsedInput { get; }
    public Position Position { get; }
    public bool EndOfInput { get; }
    public bool Success => true;
    public ErrorMessageList ErrorMessages { get; }
}
