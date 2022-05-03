namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// When parser p consumes any input, Label(p, l) is the same as p.
    /// When parser p does not consume any input, Label(p, l) is the same
    /// as p, except any messages are replaced with expectation label l.
    /// </summary>
    public static Parser<TItem, TValue> Label<TItem, TValue>(Parser<TItem, TValue> parse, string label)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var originalIndex = index;

            var value = parse(input, ref index, out succeeded, out expectation);

            if (!succeeded)
                if (originalIndex == index)
                    expectation = label;

            return value;
        };
    }
}
