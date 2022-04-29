namespace Parsley;

public readonly ref struct @int
{
    public @int(int value)
    {
        Value = value;
    }

    public int Value { get; init; }

    public override string ToString()
        => $"{Value}";
}
