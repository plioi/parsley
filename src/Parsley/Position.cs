namespace Parsley;

public readonly ref struct Position
{
    public Position(int index, int line, int column)
    {
        Index = index;
        Line = line;
        Column = column;
    }

    public int Index { get; init; }
    public int Line { get; init; }
    public int Column { get; init; }

    public override string ToString()
        => $"{Index} ({Line}, {Column})";
}
