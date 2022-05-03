namespace Parsley;

partial class Grammar
{
    public static Parser<char, string> Keyword(string word)
    {
        if (word.Any(ch => !char.IsLetter(ch)))
            throw new ArgumentException("Keywords may only contain letters.", nameof(word));

        return (ReadOnlySpan<char> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var peek = input.Peek(index, word.Length + 1);

            if (peek.StartsWith(word))
            {
                if (peek.Length == word.Length || !char.IsLetter(peek[^1]))
                {
                    index += word.Length;

                    succeeded = true;
                    expectation = null;
                    return word;
                }
            }

            succeeded = false;
            expectation = word;
            return null;
        };
    }
}
