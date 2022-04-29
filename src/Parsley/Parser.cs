using System.Diagnostics.CodeAnalysis;

namespace Parsley;

public delegate bool Parser<TValue>(ref ReadOnlySpan<char> input,
                                    ref Index index,
                                    [NotNullWhen(true)] out TValue? value,
                                    [NotNullWhen(false)] out string? expectation);
