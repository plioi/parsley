namespace Parsley;

public static class ReadOnlySpanExtensions
{
    public static ReadOnlySpan<char> Peek(this ref ReadOnlySpan<char> input, int length)
        => length >= input.Length
            ? input.Slice(0)
            : input.Slice(0, length);

    public static void Advance(this ref ReadOnlySpan<char> input, ref int index, int length)
    {
        var traversed = length > input.Length
            ? input.Length
            : length;

        input = input.Slice(traversed);

        index += traversed;
    }

    public static ReadOnlySpan<char> TakeWhile(this ref ReadOnlySpan<char> input, Predicate<char> test)
    {
        int i = 0;

        while (i < input.Length && test(input[i]))
            i++;

        return input.Peek(i);
    }
}
