namespace Parsley;

partial class Grammar
{
    internal static Parser<TItem, Void> EndOfInput<TItem>()
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            if (index == input.Length)
            {
                expectation = null;
                succeeded = true;
                return Void.Value;
            }

            expectation = "end of input";
            succeeded = false;
            return Void.Value;
        };
    }
}
