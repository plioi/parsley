namespace Parsley;

public static class ReadOnlySpanExtensions
{
    public static ReadOnlySpan<char> Peek(this ref ReadOnlySpan<char> input, int length)
        => length >= input.Length
            ? input.Slice(0)
            : input.Slice(0, length);

    public static void Advance(this ref ReadOnlySpan<char> input, ref Position position, int length)
    {
        if (length == 0)
            return;

        int lineDelta = 0;
        int columnDelta = 0;

        var peek = input.Peek(length);

        foreach (var ch in peek)
        {
            if (ch == '\n')
            {
                lineDelta++;
                columnDelta = 0 - position.Column;
            }

            columnDelta++;
        }

        input = input.Slice(peek.Length);
        position = new Position(position.Line + lineDelta, position.Column + columnDelta);
    }

    public static ReadOnlySpan<char> TakeWhile(this ref ReadOnlySpan<char> input, Predicate<char> test)
    {
        int i = 0;

        while (i < input.Length && test(input[i]))
            i++;

        return input.Peek(i);
    }
}
