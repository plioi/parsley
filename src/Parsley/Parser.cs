using System.Diagnostics.CodeAnalysis;

namespace Parsley;

public delegate bool Parser<T>(ref Text input,
                               ref Position position,
                               [NotNullWhen(true)] out T? value,
                               [NotNullWhen(false)] out string? expectation);
