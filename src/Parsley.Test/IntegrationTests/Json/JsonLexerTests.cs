using NUnit.Framework;

namespace Parsley.Test.IntegrationTests.Json
{
    [TestFixture]
    public class JsonLexerTests
    {
        [Test]
        public void RecognizesSkippableWhitespace()
        {
            AssertTokens(" ", Lexer.EndOfInput, "");
            AssertTokens("\t", Lexer.EndOfInput, "");
            AssertTokens("\n", Lexer.EndOfInput, "");
            AssertTokens("\r", Lexer.EndOfInput, "");

            AssertTokens(" \t\n\r", Lexer.EndOfInput, "");
        }

        [Test]
        public void RecognizesKeywords()
        {
            AssertTokens("null", JsonLexer.@null, "null");
            AssertTokens("true", JsonLexer.@true, "true");
            AssertTokens("false", JsonLexer.@false, "false");

            AssertTokens("null true false", "null", "true", "false");
        }

        [Test]
        public void RecognizesOperators()
        {
            AssertTokens(",", JsonLexer.Comma, ",");
            AssertTokens("[", JsonLexer.OpenArray, "[");
            AssertTokens("]", JsonLexer.CloseArray, "]");
            AssertTokens("{", JsonLexer.OpenDictionary, "{");
            AssertTokens("}", JsonLexer.CloseDictionary, "}");
            AssertTokens(":", JsonLexer.Colon, ":");

            AssertTokens(",[]{}:", ",", "[", "]", "{", "}", ":");
        }

        [Test]
        public void RecognizesQuotations()
        {
            AssertTokens("\"\"", JsonLexer.Quotation, "\"\"");
            AssertTokens("\"a\"", JsonLexer.Quotation, "\"a\"");
            AssertTokens("\"abc\"", JsonLexer.Quotation, "\"abc\"");
            AssertTokens("\"abc \\\" def\"", JsonLexer.Quotation, "\"abc \\\" def\"");
            AssertTokens("\"abc \\\\ def\"", JsonLexer.Quotation, "\"abc \\\\ def\"");
            AssertTokens("\"abc \\/ def\"", JsonLexer.Quotation, "\"abc \\/ def\"");
            AssertTokens("\"abc \\b def\"", JsonLexer.Quotation, "\"abc \\b def\"");
            AssertTokens("\"abc \\f def\"", JsonLexer.Quotation, "\"abc \\f def\"");
            AssertTokens("\"abc \\n def\"", JsonLexer.Quotation, "\"abc \\n def\"");
            AssertTokens("\"abc \\r def\"", JsonLexer.Quotation, "\"abc \\r def\"");
            AssertTokens("\"abc \\t def\"", JsonLexer.Quotation, "\"abc \\t def\"");
            AssertTokens("\"abc \\u005C def\"", JsonLexer.Quotation, "\"abc \\u005C def\"");

            AssertTokens("\" a \" \" b \" \" c \"", JsonLexer.Quotation, "\" a \"", "\" b \"", "\" c \"");
        }

        [Test]
        public void RecognizesNumbers()
        {
            AssertTokens("0", JsonLexer.Number, "0");
            AssertTokens("1", JsonLexer.Number, "1");
            AssertTokens("12345", JsonLexer.Number, "12345");
            AssertTokens("12345", JsonLexer.Number, "12345");
            AssertTokens("0.012", JsonLexer.Number, "0.012");
            AssertTokens("0e1", JsonLexer.Number, "0e1");
            AssertTokens("0e+1", JsonLexer.Number, "0e+1");
            AssertTokens("0e-1", JsonLexer.Number, "0e-1");
            AssertTokens("0E1", JsonLexer.Number, "0E1");
            AssertTokens("0E+1", JsonLexer.Number, "0E+1");
            AssertTokens("0E-1", JsonLexer.Number, "0E-1");
            AssertTokens("10e11", JsonLexer.Number, "10e11");
            AssertTokens("10.123e11", JsonLexer.Number, "10.123e11");
        }

        private static void AssertTokens(string source, TokenKind expectedKind, params string[] expectedLiterals)
        {
            Lexer lexer = new JsonLexer(source);

            foreach (var expectedLiteral in expectedLiterals)
            {
                lexer.CurrentToken.ShouldBe(expectedKind, expectedLiteral);
                lexer = lexer.Advance();
            }

            lexer.CurrentToken.Kind.ShouldEqual(Lexer.EndOfInput);
        }

        private static void AssertTokens(string source, params string[] expectedLiterals)
        {
            Lexer lexer = new JsonLexer(source);

            foreach (var expectedLiteral in expectedLiterals)
            {
                lexer.CurrentToken.Literal.ShouldEqual(expectedLiteral);
                lexer.CurrentToken.Kind.ShouldNotEqual(Lexer.Unknown);
                lexer = lexer.Advance();
            }

            lexer.CurrentToken.Kind.ShouldEqual(Lexer.EndOfInput);
        }
    }
}