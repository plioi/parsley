using Xunit;

namespace Parsley.IntegrationTests.Json
{
    public class JsonLexerTests
    {
        [Fact]
        public void RecognizesSkippableWhitespace()
        {
            new JsonLexer(" ").ShouldYieldTokens(Lexer.EndOfInput, "");
            new JsonLexer("\t").ShouldYieldTokens(Lexer.EndOfInput, "");
            new JsonLexer("\n").ShouldYieldTokens(Lexer.EndOfInput, "");
            new JsonLexer("\r").ShouldYieldTokens(Lexer.EndOfInput, "");

            new JsonLexer(" \t\n\r").ShouldYieldTokens(Lexer.EndOfInput, "");
        }

        [Fact]
        public void RecognizesKeywords()
        {
            new JsonLexer("null").ShouldYieldTokens(JsonLexer.@null, "null");
            new JsonLexer("true").ShouldYieldTokens(JsonLexer.@true, "true");
            new JsonLexer("false").ShouldYieldTokens(JsonLexer.@false, "false");

            new JsonLexer("null true false").ShouldYieldTokens("null", "true", "false");
        }

        [Fact]
        public void RecognizesOperators()
        {
            new JsonLexer(",").ShouldYieldTokens(JsonLexer.Comma, ",");
            new JsonLexer("[").ShouldYieldTokens(JsonLexer.OpenArray, "[");
            new JsonLexer("]").ShouldYieldTokens(JsonLexer.CloseArray, "]");
            new JsonLexer("{").ShouldYieldTokens(JsonLexer.OpenDictionary, "{");
            new JsonLexer("}").ShouldYieldTokens(JsonLexer.CloseDictionary, "}");
            new JsonLexer(":").ShouldYieldTokens(JsonLexer.Colon, ":");

            new JsonLexer(",[]{}:").ShouldYieldTokens(",", "[", "]", "{", "}", ":");
        }

        [Fact]
        public void RecognizesQuotations()
        {
            new JsonLexer("\"\"").ShouldYieldTokens(JsonLexer.Quotation, "\"\"");
            new JsonLexer("\"a\"").ShouldYieldTokens(JsonLexer.Quotation, "\"a\"");
            new JsonLexer("\"abc\"").ShouldYieldTokens(JsonLexer.Quotation, "\"abc\"");
            new JsonLexer("\"abc \\\" def\"").ShouldYieldTokens(JsonLexer.Quotation, "\"abc \\\" def\"");
            new JsonLexer("\"abc \\\\ def\"").ShouldYieldTokens(JsonLexer.Quotation, "\"abc \\\\ def\"");
            new JsonLexer("\"abc \\/ def\"").ShouldYieldTokens(JsonLexer.Quotation, "\"abc \\/ def\"");
            new JsonLexer("\"abc \\b def\"").ShouldYieldTokens(JsonLexer.Quotation, "\"abc \\b def\"");
            new JsonLexer("\"abc \\f def\"").ShouldYieldTokens(JsonLexer.Quotation, "\"abc \\f def\"");
            new JsonLexer("\"abc \\n def\"").ShouldYieldTokens(JsonLexer.Quotation, "\"abc \\n def\"");
            new JsonLexer("\"abc \\r def\"").ShouldYieldTokens(JsonLexer.Quotation, "\"abc \\r def\"");
            new JsonLexer("\"abc \\t def\"").ShouldYieldTokens(JsonLexer.Quotation, "\"abc \\t def\"");
            new JsonLexer("\"abc \\u005C def\"").ShouldYieldTokens(JsonLexer.Quotation, "\"abc \\u005C def\"");

            new JsonLexer("\" a \" \" b \" \" c \"").ShouldYieldTokens(JsonLexer.Quotation, "\" a \"", "\" b \"", "\" c \"");
        }

        [Fact]
        public void RecognizesNumbers()
        {
            new JsonLexer("0").ShouldYieldTokens(JsonLexer.Number, "0");
            new JsonLexer("1").ShouldYieldTokens(JsonLexer.Number, "1");
            new JsonLexer("12345").ShouldYieldTokens(JsonLexer.Number, "12345");
            new JsonLexer("12345").ShouldYieldTokens(JsonLexer.Number, "12345");
            new JsonLexer("0.012").ShouldYieldTokens(JsonLexer.Number, "0.012");
            new JsonLexer("0e1").ShouldYieldTokens(JsonLexer.Number, "0e1");
            new JsonLexer("0e+1").ShouldYieldTokens(JsonLexer.Number, "0e+1");
            new JsonLexer("0e-1").ShouldYieldTokens(JsonLexer.Number, "0e-1");
            new JsonLexer("0E1").ShouldYieldTokens(JsonLexer.Number, "0E1");
            new JsonLexer("0E+1").ShouldYieldTokens(JsonLexer.Number, "0E+1");
            new JsonLexer("0E-1").ShouldYieldTokens(JsonLexer.Number, "0E-1");
            new JsonLexer("10e11").ShouldYieldTokens(JsonLexer.Number, "10e11");
            new JsonLexer("10.123e11").ShouldYieldTokens(JsonLexer.Number, "10.123e11");
        }
    }
}