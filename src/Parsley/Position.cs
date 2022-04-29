namespace Parsley;

public readonly ref struct Position
{
    public Position(int value)
    {
        Value = value;
    }

    public int Value { get; init; }

    public override string ToString()
        => $"{Value}";
}
