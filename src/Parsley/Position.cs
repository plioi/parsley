namespace Parsley;

public record struct Position(int Line, int Column)
{
    public override string ToString()
        => $"({Line}, {Column})";
}
