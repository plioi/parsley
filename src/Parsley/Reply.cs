namespace Parsley;

public interface Reply<out T>
{
    T Value { get; }
    Text UnparsedInput { get; }
    bool Success { get; }
    ErrorMessageList ErrorMessages { get; }
}
