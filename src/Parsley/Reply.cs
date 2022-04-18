namespace Parsley;

public interface Reply<out T>
{
    T Value { get; }
    bool Success { get; }
    string Expectation { get; }
}
