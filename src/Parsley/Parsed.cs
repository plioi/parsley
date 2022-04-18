namespace Parsley;

public class Parsed<T> : Reply<T>
{
    public Parsed(T value)
        => Value = value;

    public T Value { get; }
    public bool Success => true;
    public string Expectation => throw new MemberAccessException("Cannot access Expectation for a Parsed reply.");
}
