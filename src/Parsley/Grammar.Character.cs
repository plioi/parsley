using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    public static Parser<char> Character(char expected)
    {
        return Character(x => x == expected, expected.ToString());
    }

    public static Parser<char> Character(Predicate<char> test, string name)
    {
        return (ref Text input, ref Position position, out char value, [NotNullWhen(false)] out string? expectation) =>
        {
            var next = input.Peek(1);

            if (next.Length == 1)
            {
                char c = next[0];
                if (test(c))
                {
                    var positionDelta = input.Advance(1);
                    position.Move(positionDelta);

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

