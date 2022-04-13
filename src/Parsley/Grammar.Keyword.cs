namespace Parsley;

partial class Grammar
{
    public static Parser<string> Keyword(string word)
    {
        if (word.Any(ch => !char.IsLetter(ch)))
            throw new ArgumentException("Keywords may only contain letters.", nameof(word));

        return input =>
        {
            var peek = input.Peek(word.Length + 1);

            if (peek.StartsWith(word, StringComparison.Ordinal))
            {
                if (peek.Length == word.Length || !char.IsLetter(peek[^1]))
                {
                    input.Advance(word.Length);

                    return new Parsed<string>(word, input.Position);
                }
            }

            return new Error<string>(input.Position, ErrorMessage.Expected(word));
        };
    }
}
