namespace Parsley;

public interface Reply<out T>
{
    T Value { get; }
    Position Position { get; }
    bool Success { get; }
    ErrorMessageList ErrorMessages { get; }
}
