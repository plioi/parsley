using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    public static Parser<TItem, TItem> Single<TItem>(TItem expected)
    {
        return Single<TItem>(x => EqualityComparer<TItem>.Default.Equals(x, expected), $"{expected}");
    }

    public static Parser<TItem, TItem> Single<TItem>(Func<TItem, bool> test, string name)
    {
        return (ReadOnlySpan<TItem> input, ref int index, [NotNullWhen(true)] out TItem? value, [NotNullWhen(false)] out string? expectation) =>
        {
            if (index + 1 <= input.Length)
            {
                var c = input[index]!;
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

