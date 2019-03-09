using System.Collections.Generic;
using System.IO;

namespace Parsimonious
{
    public class LinedLexer : LexerBase<TextReader>
    {
        public LinedLexer(params TokenKind[] kinds)
            : base(kinds)
        { }

        public override IEnumerable<Token> Tokenize(TextReader textReader)
        {
            var text = new LinedText(textReader);

            while (text.ReadLine())
                while (!text.EndOfLine)
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