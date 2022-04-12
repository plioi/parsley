namespace Parsley.Primitives;

class KeywordParser : PatternParser
{
    public KeywordParser(string word)
        : base(word, word + @"\b")
    {
        if (word.Any(ch => !char.IsLetter(ch)))
            throw new ArgumentException("Keywords may only contain letters.", nameof(word));
    }
}
