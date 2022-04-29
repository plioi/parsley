namespace Parsley;

public static class ReadOnlySpanExtensions
{
    public static ReadOnlySpan<T> Peek<T>(this ReadOnlySpan<T> input, int index, int length)
        => index + length > input.Length
            ? input.Slice(index)
            : input.Slice(index, length);

    public static ReadOnlySpan<T> TakeWhile<T>(this ReadOnlySpan<T> input, int index, Predicate<T> test)
    {
        int i = 0;

        while (index + i < input.Length && test(input[index + i]))
            i++;

        return input.Peek(index, i);
    }
}
