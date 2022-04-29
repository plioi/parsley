namespace Parsley;

public readonly ref struct Index
{
    public Index(int value)
    {
        Value = value;
    }

    public int Value { get; init; }

    public override string ToString()
        => $"{Value}";
}
