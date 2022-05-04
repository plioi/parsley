namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// The parser Recursive(() => p) behaves like p while enabling p to be recursive.
    /// </summary>
    public static Parser<TItem, TValue> Recursive<TItem, TValue>(Func<Parser<TItem, TValue>> getRecursiveParser)
    {
        var delayedParserReference = new Lazy<Parser<TItem, TValue>>(getRecursiveParser);

        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation)
            => delayedParserReference.Value(input, ref index, out succeeded, out expectation);
    }
}
