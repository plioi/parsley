namespace Parsley;

public delegate TValue? Inspector<TItem, out TValue>(ReadOnlySpan<TItem> input, int index);
