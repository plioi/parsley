namespace Parsley;

partial class Grammar
{
    public static Parser<TItem, TItem> Single<TItem>(TItem expected, string? name = null)
    {
        return Single<TItem>(x => EqualityComparer<TItem>.Default.Equals(x, expected), name ?? $"{expected}");
    }

    public static Parser<TItem, TItem> Single<TItem>(Func<TItem, bool> test, string name)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            if (index + 1 <= input.Length)
            {
                var c = input[index]!;
                if (test(c))
                {
                    index += 1;

                    expectation = null;
                    succeeded = true;
                    return c;
                }
            }

            expectation = name;
            succeeded = false;
            return default;
        };
    }
}

