namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// The parser Not(p) succeeds when parser p fails, and fails
    /// when parser p succeeds. Not(p) never consumes input.
    /// </summary>
    public static Parser<TItem, Void> Not<TItem, TValue>(Parser<TItem, TValue> parse)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var copyOfIndex = index;

            var ignoredValue = parse(input, ref copyOfIndex, out var parseSucceeded, out _);

            if (parseSucceeded)
            {
                expectation = "parse failure";
                succeeded = false;
            }
            else
            {
                expectation = null;
                succeeded = true;
            }

            return Void.Value;
        };
    }
}
