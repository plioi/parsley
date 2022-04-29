using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    public static Parser<char> Single(char expected)
    {
        return Single(x => x == expected, expected.ToString());
    }

    public static Parser<char> Single(Predicate<char> test, string name)
    {
        return (ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out char value, [NotNullWhen(false)] out string? expectation) =>
        {
            var next = input.Peek(index, 1);

            if (next.Length == 1)
            {
                char c = next[0];
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

