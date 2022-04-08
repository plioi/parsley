using System.Text.RegularExpressions;

namespace Parsley;

public abstract class TokenKind
{
    public static readonly TokenKind EndOfInput = new Empty("end of input");

    readonly string name;

    protected TokenKind(string name)
    {
        this.name = name;
    }

    public bool TryMatch(Text text, out Token token)
    {
        var match = Match(text);

        if (match.Success)
        {
            token = new Token(this, match.Value);
            return true;
        }

        token = null;
        return false;
    }

    protected abstract MatchResult Match(Text text);

    public string Name => name;

    public override string ToString() => name;
}

public class Pattern : TokenKind
{
    readonly TokenRegex regex;

    public Pattern(string name, string pattern, params RegexOptions[] regexOptions)
        : base(name)
    {
        regex = new TokenRegex(pattern, regexOptions);
    }

    protected override MatchResult Match(Text text)
        => text.Match(regex);
}

public class Keyword : Pattern
{
    public Keyword(string word)
        : base(word, word + @"\b")
    {
        if (word.Any(ch => !char.IsLetter(ch)))
            throw new ArgumentException("Keywords may only contain letters.", nameof(word));
    }
}

public class Operator : TokenKind
{
    readonly string symbol;

    public Operator(string symbol)
        : base(symbol)
    {
        this.symbol = symbol;
    }

    protected override MatchResult Match(Text text)
    {
        var peek = text.Peek(symbol.Length);

        if (peek == symbol)
            return MatchResult.Succeed(peek);

        return MatchResult.Fail;
    }
}

public class Empty : TokenKind
{
    public Empty(string name)
        : base(name) { }

    protected override MatchResult Match(Text text)
    {
        if (text.EndOfInput)
            return MatchResult.Succeed("");

        return MatchResult.Fail;
    }
}
