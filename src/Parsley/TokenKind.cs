using System.Text.RegularExpressions;

namespace Parsley;

public abstract class TokenKind : IParser<Token>
{
    public abstract Reply<Token> Parse(Text input);
}

public class Pattern : TokenKind
{
    readonly string name;
    readonly TokenRegex regex;

    public Pattern(string name, string pattern, params RegexOptions[] regexOptions)
    {
        this.name = name;
        regex = new TokenRegex(pattern, regexOptions);
    }

    public override Reply<Token> Parse(Text input)
    {
        var match = input.Match(regex);

        if (match.Success)
        {
            var token = new Token(this, match.Value);

            return new Parsed<Token>(token, input.Advance(token.Literal.Length));
        }

        return new Error<Token>(input, ErrorMessage.Expected(name));
    }
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
        => this.symbol = symbol;

    public override Reply<Token> Parse(Text input)
    {
        var peek = input.Peek(symbol.Length);

        if (peek == symbol)
        {
            var token = new Token(this, peek);

            return new Parsed<Token>(token, input.Advance(peek.Length));
        }

        return new Error<Token>(input, ErrorMessage.Expected(symbol));
    }
}
