using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    public static Parser<string> Keyword(string word)
    {
        if (word.Any(ch => !char.IsLetter(ch)))
            throw new ArgumentException("Keywords may only contain letters.", nameof(word));

        return (ref Text input, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var peek = input.Peek(word.Length + 1);

            if (peek.StartsWith(word))
            {
                if (peek.Length == word.Length || !char.IsLetter(peek[^1]))
                {
                    input.Advance(word.Length);

                    expectation = null;
                    value = word;
                    return true;
                }
            }

            expectation = word;
            value = null;
            return false;
        };
    }
}
