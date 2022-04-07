namespace Parsley;

public interface Reply<out T>
{
    T Value { get; }
    Input UnparsedInput { get; }
    bool Success { get; }
    ErrorMessageList ErrorMessages { get; }
}
