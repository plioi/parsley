namespace Parsley;

public static class ReadOnlySpanExtensions
{
    public static ReadOnlySpan<T> Peek<T>(this ReadOnlySpan<T> input, int index, int length)
        => index + length > input.Length
            ? input.Slice(index)
            : input.Slice(index, length);

    public static int CountWhile<T>(this ReadOnlySpan<T> input, int index, Func<T, bool> test)
    {
        int length = 0;

        while (index + length < input.Length && test(input[index + length]))
            length++;

        return length;
    }

    public static int CountWhile<T>(this ReadOnlySpan<T> input, int index, Func<T, bool> test, int maxCount)
    {
        int length = 0;

        while (length < maxCount && index + length < input.Length && test(input[index + length]))
            length++;

        return length;
    }
}
