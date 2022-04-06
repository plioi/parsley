namespace Parsley
{
    using System.Collections.Generic;
    using System.Linq;

    public class Lexer
    {
        private readonly List<TokenKind> kinds;

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

        private Token GetToken(Text text)
        {
            Token token;
            foreach (var kind in kinds)
                if (kind.TryMatch(text, out token))
                    return token;

            return null; //Unknown guarantees this is reachable only at the end of input.
        }
    }
}