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
        return (ref ReadOnlySpan<char> input, ref int position, [NotNullWhen(true)] out char value, [NotNullWhen(false)] out string? expectation) =>
        {
            var next = input.Peek(1);

            if (next.Length == 1)
            {
                char c = next[0];
                if (test(c))
                {
                    input.Advance(ref position, 1);

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

