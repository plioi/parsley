namespace Parsley;

public static class ReadOnlySpanExtensions
{
    public static ReadOnlySpan<char> Peek(this ref ReadOnlySpan<char> input, int length)
        => length >= input.Length
            ? input.Slice(0)
            : input.Slice(0, length);

    public static void Advance(this ref ReadOnlySpan<char> input, ref Position position, int length)
    {
        var traversed = input.Advance(length);

        position = Advance(position, traversed);
    }

    static ReadOnlySpan<char> Advance(this ref ReadOnlySpan<char> input, int length)
    {
        var peek = input.Peek(length);

        input = input.Slice(peek.Length);

        return peek;
    }

    static Position Advance(Position position, ReadOnlySpan<char> traversed)
    {
        int lineDelta = 0;
        int columnDelta = 0;

        foreach (var ch in traversed)
        {
            if (ch == '\n')
            {
                lineDelta++;
                columnDelta = 0 - position.Column;
            }

            columnDelta++;
        }

        return new Position(position.Line + lineDelta, position.Column + columnDelta);
    }

    public static ReadOnlySpan<char> TakeWhile(this ref ReadOnlySpan<char> input, Predicate<char> test)
    {
        int i = 0;

        while (i < input.Length && test(input[i]))
            i++;

        return input.Peek(i);
    }
}
