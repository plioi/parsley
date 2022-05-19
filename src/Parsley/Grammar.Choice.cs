namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// Attempts the given parsers from left to right.
    ///
    /// <para>
    /// If a parser succeeds, that result wins. If a parser fails without
    /// consuming input, the next parser is attempted. If a parser fails after
    /// consuming input, subsequent parsers will not be attempted and
    /// the overall missed expectation will be that of the offending individual
    /// parser. If all parsers fail without consuming input, the last failing parser's
    /// expectation will be assumed. To override this assumption with a
    /// comprehensive expectation, see the overload which accepts a <c>name</c>
    /// string.
    /// </para>
    /// </summary>
    public static Parser<TItem, TValue> Choice<TItem, TValue>(params Parser<TItem, TValue>[] parsers)
        => ChoiceCore(name: null, parsers);

    /// <summary>
    /// Attempts the given parsers from left to right, using the given <c>name</c>
    /// for a comprehensive expectation in the case that all parsers fail without
    /// consuming input.
    ///
    /// <para>
    /// If a parser succeeds, that result wins. If a parser fails without
    /// consuming input, the next parser is attempted. If a parser fails after
    /// consuming input, subsequent parsers will not be attempted and
    /// the overall missed expectation will be that of the offending individual
    /// parser. If all parsers fail without consuming input, the given <c>name</c>
    /// will be used as the comprehensive missed expectation.
    /// </para>
    /// </summary>
    public static Parser<TItem, TValue> Choice<TItem, TValue>(string name, params Parser<TItem, TValue>[] parsers)
        => ChoiceCore(name, parsers);

    static Parser<TItem, TValue> ChoiceCore<TItem, TValue>(string? name, Parser<TItem, TValue>[] parsers)
    {
        if (parsers.Length <= 1)
            throw new ArgumentException(
                $"{nameof(Choice)} requires at least two parsers to choose between.", nameof(parsers));

        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var originalIndex = index;
            expectation = null;

            foreach (var parser in parsers)
            {
                var value = parser(input, ref index, out succeeded, out expectation);

                if (succeeded)
                    return value;

                if (originalIndex != index)
                    return default;
            }

            succeeded = false;

            if (name != null)
                expectation = name;

            return default;
        };
    }
}
