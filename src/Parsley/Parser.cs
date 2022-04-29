using System.Diagnostics.CodeAnalysis;

namespace Parsley;

public delegate bool Parser<TValue>(ReadOnlySpan<char> input,
                                    ref int index,
                                    [NotNullWhen(true)] out TValue? value,
                                    [NotNullWhen(false)] out string? expectation);
