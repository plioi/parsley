using System.Diagnostics.CodeAnalysis;

namespace Parsley;

public delegate bool Parser<TItem, TValue>(
    ReadOnlySpan<TItem> input,
    ref int index,
    [NotNullWhen(true)] out TValue? value,
    [NotNullWhen(false)] out string? expectation);
