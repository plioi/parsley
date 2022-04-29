using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    public static Parser<TItem, TItem> Single<TItem>(TItem expected)
    {
        return Single<TItem>(x => EqualityComparer<TItem>.Default.Equals(x, expected), $"{expected}");
    }

    public static Parser<TItem, TItem> Single<TItem>(Predicate<TItem> test, string name)
    {
        return (ReadOnlySpan<TItem> input, ref int index, [NotNullWhen(true)] out TItem? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var next = input.Peek(index, 1);

            if (next.Length == 1)
            {
                var c = next[0]!;
                if (test(c))
                {
                    index += 1;

                    expectation = null;
                    value = c;
                    return true;
                }
            }

            expectation = name;
            value = default;
            return false;
        };
    }
}

