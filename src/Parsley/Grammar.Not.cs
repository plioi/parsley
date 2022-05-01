using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// The parser Not(p) succeeds when parser p fails, and fails
    /// when parser p succeeds. Not(p) never consumes input.
    /// </summary>
    public static Parser<TItem, Void> Not<TItem, TValue>(Parser<TItem, TValue> parse)
    {
        return (ReadOnlySpan<TItem> input, ref int index, [NotNullWhen(true)] out Void value, [NotNullWhen(false)] out string? expectation) =>
        {
            value = Void.Value;

            var copyOfIndex = index;

            if (parse(input, ref copyOfIndex, out _, out _))
            {
                expectation = "parse failure";
                return false;
            }

            expectation = null;
            return true;
        };
    }
}
