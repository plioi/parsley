namespace Parsley;

public static class ReadOnlySpanExtensions
{
    public static ReadOnlySpan<char> Peek(this ref ReadOnlySpan<char> input, int length)
        => length >= input.Length
            ? input.Slice(0)
            : input.Slice(0, length);

    public static void Advance(this ref ReadOnlySpan<char> input, ref int index, int length)
    {
        var traversed = input.Advance(length);

        index += traversed.Length;
    }

    static ReadOnlySpan<char> Advance(this ref ReadOnlySpan<char> input, int length)
    {
        var peek = input.Peek(length);

        input = input.Slice(peek.Length);

        return peek;
    }

    public static ReadOnlySpan<char> TakeWhile(this ref ReadOnlySpan<char> input, Predicate<char> test)
    {
        int i = 0;

        while (i < input.Length && test(input[i]))
            i++;

        return input.Peek(i);
    }
}
