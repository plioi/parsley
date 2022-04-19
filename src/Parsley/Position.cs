namespace Parsley;

public record struct Position(int Line, int Column)
{
    public void Move((int lineDelta, int columnDelta) delta)
    {
        Line += delta.lineDelta;
        Column += delta.columnDelta;
    }

    public override string ToString()
        => $"({Line}, {Column})";
}
