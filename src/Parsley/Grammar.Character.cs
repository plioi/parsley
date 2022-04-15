namespace Parsley;

partial class Grammar
{
    public static Parser<char> Character(char expected)
    {
        return Character(x => x == expected, expected.ToString());
    }

    public static Parser<char> Character(Predicate<char> test, string name)
    {
        return (ref Text input) =>
        {
            var next = input.Peek(1);

            if (next.Length == 1)
            {
                char c = next[0];
                if (test(c))
                {
                    input.Advance(1);

                    return new Parsed<char>(c);
                }
            }

            return new Error<char>(ErrorMessage.Expected(name));
        };
    }
}

