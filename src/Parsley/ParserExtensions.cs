using System.Diagnostics.CodeAnalysis;

namespace Parsley;

public static class ParserExtensions
{
    public static bool TryParse<TItem, TValue>(
        this Parser<TItem, TValue> parse,
        ReadOnlySpan<TItem> input,
        [NotNullWhen(true)] out TValue? value,
        [NotNullWhen(false)] out ParseError? error)
    {
        var parseToEnd =
            from result in parse
            from end in Grammar.EndOfInput<TItem>()
            select result;

        return TryPartialParse(parseToEnd, input, out int index, out value, out error);
    }

    public static bool TryPartialParse<TItem, TValue>(
        this Parser<TItem, TValue> parse,
        ReadOnlySpan<TItem> input,
        out int index,
        [NotNullWhen(true)] out TValue? value,
        [NotNullWhen(false)] out ParseError? error)
    {
        index = 0;
        value = parse(input, ref index, out var succeeded, out var expectation);

        if (succeeded)
        {
            error = null;
            #pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
            return true;
            #pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
        }

        error = new ParseError(index, expectation!);
        return false;
    }
}
