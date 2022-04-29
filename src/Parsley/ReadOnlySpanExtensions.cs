namespace Parsley;

public static class ReadOnlySpanExtensions
{
    public static ReadOnlySpan<char> Peek(this in ReadOnlySpan<char> input, int index, int length)
        => index + length > input.Length
            ? input.Slice(index)
            : input.Slice(index, length);

    public static ReadOnlySpan<char> TakeWhile(this in ReadOnlySpan<char> input, int index, Predicate<char> test)
    {
        int i = 0;

        while (index + i < input.Length && test(input[index + i]))
            i++;

        return input.Peek(index, i);
    }
}
