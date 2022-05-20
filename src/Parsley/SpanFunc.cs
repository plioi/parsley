namespace Parsley;

public delegate TResult SpanFunc<TItem, out TResult>(ReadOnlySpan<TItem> input);
