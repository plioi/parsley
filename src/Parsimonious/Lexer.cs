using System.Collections.Generic;

namespace Parsimonious
{
    public class Lexer : LexerBase<string>
    {
        public Lexer(params TokenKind[] kinds)
            : base(kinds)
        { }

        public override IEnumerable<Token> Tokenize(string input)
        {
            var text = new Text(input);

            while (!text.EndOfInput)
            {
                var current = GetToken(text);

                if (current == null)
                {
                    yield return MatchUnknownToken(text);
                    yield break;
                }

                text.Advance(current.Literal.Length);

                if (current.Kind.Skippable)
                    continue;

                yield return current;
            }
        }
    }
}