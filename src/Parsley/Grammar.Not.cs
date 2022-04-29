using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// The parser Not(p) succeeds when parser p fails, and fails
    /// when parser p succeeds. Not(p) never consumes input.
    /// </summary>
    public static Parser_char_<Void> Not<TValue>(Parser_char_<TValue> parse)
    {
        return (ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out Void value, [NotNullWhen(false)] out string? expectation) =>
        {
            value = Void.Value;

            var copyOfIndex = index;

            var succeeded = parse(input, ref copyOfIndex, out _, out expectation);

            if (succeeded)
            {
                expectation = "parse failure";
                return false;
            }

            return true;
        };
    }
}
