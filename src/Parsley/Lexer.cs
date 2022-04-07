namespace Parsley;

public class Lexer
{
    readonly List<TokenKind> kinds;

    public Lexer(params TokenKind[] kinds)
    {
        this.kinds = kinds.ToList();
        this.kinds.Add(TokenKind.Unknown);
    }

    public IEnumerable<Token> Tokenize(string input)
    {
        var text = new Text(input);
        while (!text.EndOfInput)
        {
            var current = GetToken(text);

            //After exiting this loop, Current will be the
            //next unskippable token, and text will indicate
            //where that token starts.
            while (current.Kind.Skippable)
            {
                text = text.Advance(current.Literal.Length);

                if (text.EndOfInput)
                    yield break;

                current = GetToken(text);
            }

            text = text.Advance(current.Literal.Length);

            yield return current;
        }
    }

    Token GetToken(Text text)
    {
        foreach (var kind in kinds)
            if (kind.TryMatch(text, out var token))
                return token;

        return null; //Unknown guarantees this is reachable only at the end of input.
    }
}
