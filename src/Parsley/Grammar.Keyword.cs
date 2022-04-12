namespace Parsley;

partial class Grammar
{
    public static Parser<string> Keyword(string word)
    {
        if (word.Any(ch => !char.IsLetter(ch)))
            throw new ArgumentException("Keywords may only contain letters.", nameof(word));

        return Pattern(word, word + @"\b");
    }
}
