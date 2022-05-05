namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// The parser Inspect(...) inspects the current parser state and returns some calculation
    /// based on that state, always succeeds, and never consumes input.
    ///
    /// Note that when the end of input has been reached the index will be equal to the length
    /// of the original input, indicating the valid index at which there is nothing more to parse.
    /// In other words, it is always safe to call Slice on the `input` starting at `index`,
    /// though you may only be able to take a zero-width slice from that starting index.
    /// </summary>
    public static Parser<TItem, TValue> Inspect<TItem, TValue>(Inspector<TItem, TValue> inspect)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            expectation = null;
            succeeded = true;
            return inspect(input, index);
        };
    }

    /// <summary>
    /// The parser Index inspects the current parser state and returns the current index,
    /// always succeeds, and never consumes input. Note that when the end of input has been
    /// reached the index will be equal to the length of the original input, indicating the
    /// valid index at which there is nothing more to parse.
    ///
    /// This is an optimized alternative to Inspect((input, index) => index).
    /// </summary>
    public static Parser<TItem, int> Index<TItem>()
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            expectation = null;
            succeeded = true;
            return index;
        };
    }
}
