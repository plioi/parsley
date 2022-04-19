using System.Diagnostics.CodeAnalysis;

namespace Parsley;

public delegate bool Parser<TValue>(ref ReadOnlySpan<char> input,
                                    ref Position position,
                                    [NotNullWhen(true)] out TValue? value,
                                    [NotNullWhen(false)] out string? expectation);
