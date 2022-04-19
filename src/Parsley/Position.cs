namespace Parsley;

public readonly ref struct Position
{
    public Position(int line, int column)
    {
        Line = line;
        Column = column;
    }

    public int Line { get; init; }
    public int Column { get; init; }

    public override string ToString()
        => $"({Line}, {Column})";
}
