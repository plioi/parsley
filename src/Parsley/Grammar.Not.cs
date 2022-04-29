using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// The parser Not(p) succeeds when parser p fails, and fails
    /// when parser p succeeds. Not(p) never consumes input.
    /// </summary>
    public static Parser<Void> Not<TValue>(Parser<TValue> parse)
    {
        return (ref ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out Void value, [NotNullWhen(false)] out string? expectation) =>
        {
            value = Void.Value;

            var copyOfInput = input;
            var copyOfIndex = index;

            var succeeded = parse(ref copyOfInput, ref copyOfIndex, out _, out expectation);

            if (succeeded)
            {
                expectation = "parse failure";
                return false;
            }

            return true;
        };
    }
}
