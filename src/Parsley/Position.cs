namespace Parsley;

public struct Position
{
    public Position(int line, int column)
    {
        Line = line;
        Column = column;
    }

    public int Line { get; set; }
    public int Column { get; set; }

    public override string ToString()
        => $"({Line}, {Column})";
}
