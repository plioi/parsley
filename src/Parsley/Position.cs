namespace Parsley;

public readonly ref struct Position
{
    public Position(int index)
    {
        Index = index;
    }

    public int Index { get; init; }

    public override string ToString()
        => $"{Index}";
}
