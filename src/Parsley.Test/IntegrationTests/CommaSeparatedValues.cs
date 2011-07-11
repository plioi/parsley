using System.Linq;
using NUnit.Framework;

namespace Parsley.IntegrationTests
{
    [TestFixture]
    public class CommaSeparatedValues
    {
        [Test]
        public void Empty()
        {
            const string input = "";
            var tokens = new CsvLexer(input);
            CsvGrammar.CsvFile.Parses(tokens).IntoValue(new string[][] { });
        }

        [Test]
        public void SingleValue()
        {
            const string input = "0";
            var tokens = new CsvLexer(input);
            CsvGrammar.CsvFile.Parses(tokens).IntoValue(new[]
            {
                new[]{ "0" }
            });
        }

        [Test]
        public void DanglingComma()
        {
            const string input = "0,";
            var tokens = new CsvLexer(input);
            CsvGrammar.CsvFile.FailsToParse(tokens, "").WithMessage("(1, 3): value expected");
        }

        [Test]
        public void MultipleValues()
        {
            const string input = "abc,123,DEF";
            var tokens = new CsvLexer(input);
            CsvGrammar.CsvFile.Parses(tokens).IntoValue(new[]
            {
                new[] { "abc", "123", "DEF" }
            });
        }

        [Test]
        public void MultipleLines()
        {
            const string input = "abc,123,DEF\r\nghi,456,JKL\r\n";
            var tokens = new CsvLexer(input);
            CsvGrammar.CsvFile.Parses(tokens).IntoValue(new[]
            {
                new[] {"abc", "123", "DEF"},
                new[] {"ghi", "456", "JKL"}
            });
        }

        private class CsvLexer : Lexer
        {
            public static readonly TokenKind Comma = new TokenKind("comma", @",");
            public static readonly TokenKind EndOfLine = new TokenKind("end of line", @"\r\n");
            public static readonly TokenKind Value = new TokenKind("value", @"[a-zA-Z0-9]+");

            public CsvLexer(string source)
                : base(new Text(source), Comma, EndOfLine, Value) { }
        }

        private class CsvGrammar : Grammar
        {
            public static Parser<string[][]> CsvFile
            {
                get
                {
                    return from lines in ZeroOrMore(Line)
                           from eoi in EndOfInput
                           select lines.ToArray();
                }
            }

            private static Parser<string[]> Line
            {
                get
                {
                    return from values in OneOrMore(Value, Comma)
                           from eol in EndOfLine
                           select values.Select(token => token.Literal).ToArray();
                }
            }

            private static Parser<Token> EndOfLine
            {
                get { return Choice(Token(CsvLexer.EndOfLine), Token(Lexer.EndOfInput)); }
            }

            private static Parser<Token> Comma
            {
                get { return Token(","); }
            }

            private static Parser<Token> Value
            {
                get { return Token(CsvLexer.Value); }
            }
        }
    }
}