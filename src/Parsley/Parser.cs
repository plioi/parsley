namespace Parsley;

public delegate TValue? Parser<TItem, TValue>(
    ReadOnlySpan<TItem> input,
    ref int index,
    out bool succeeded,
    out string? expectation);
