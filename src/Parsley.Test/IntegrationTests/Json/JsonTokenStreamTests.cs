using Xunit;

namespace Parsley.IntegrationTests.Json
{
    public class JsonTokenStreamTests
    {
        [Fact]
        public void RecognizesSkippableWhitespace()
        {
            new JsonTokenStream(" ").ShouldYieldTokens(TokenKind.EndOfInput, "");
            new JsonTokenStream("\t").ShouldYieldTokens(TokenKind.EndOfInput, "");
            new JsonTokenStream("\n").ShouldYieldTokens(TokenKind.EndOfInput, "");
            new JsonTokenStream("\r").ShouldYieldTokens(TokenKind.EndOfInput, "");

            new JsonTokenStream(" \t\n\r").ShouldYieldTokens(TokenKind.EndOfInput, "");
        }

        [Fact]
        public void RecognizesKeywords()
        {
            new JsonTokenStream("null").ShouldYieldTokens(JsonTokenStream.@null, "null");
            new JsonTokenStream("true").ShouldYieldTokens(JsonTokenStream.@true, "true");
            new JsonTokenStream("false").ShouldYieldTokens(JsonTokenStream.@false, "false");

            new JsonTokenStream("null true false").ShouldYieldTokens("null", "true", "false");
        }

        [Fact]
        public void RecognizesOperators()
        {
            new JsonTokenStream(",").ShouldYieldTokens(JsonTokenStream.Comma, ",");
            new JsonTokenStream("[").ShouldYieldTokens(JsonTokenStream.OpenArray, "[");
            new JsonTokenStream("]").ShouldYieldTokens(JsonTokenStream.CloseArray, "]");
            new JsonTokenStream("{").ShouldYieldTokens(JsonTokenStream.OpenDictionary, "{");
            new JsonTokenStream("}").ShouldYieldTokens(JsonTokenStream.CloseDictionary, "}");
            new JsonTokenStream(":").ShouldYieldTokens(JsonTokenStream.Colon, ":");

            new JsonTokenStream(",[]{}:").ShouldYieldTokens(",", "[", "]", "{", "}", ":");
        }

        [Fact]
        public void RecognizesQuotations()
        {
            new JsonTokenStream("\"\"").ShouldYieldTokens(JsonTokenStream.Quotation, "\"\"");
            new JsonTokenStream("\"a\"").ShouldYieldTokens(JsonTokenStream.Quotation, "\"a\"");
            new JsonTokenStream("\"abc\"").ShouldYieldTokens(JsonTokenStream.Quotation, "\"abc\"");
            new JsonTokenStream("\"abc \\\" def\"").ShouldYieldTokens(JsonTokenStream.Quotation, "\"abc \\\" def\"");
            new JsonTokenStream("\"abc \\\\ def\"").ShouldYieldTokens(JsonTokenStream.Quotation, "\"abc \\\\ def\"");
            new JsonTokenStream("\"abc \\/ def\"").ShouldYieldTokens(JsonTokenStream.Quotation, "\"abc \\/ def\"");
            new JsonTokenStream("\"abc \\b def\"").ShouldYieldTokens(JsonTokenStream.Quotation, "\"abc \\b def\"");
            new JsonTokenStream("\"abc \\f def\"").ShouldYieldTokens(JsonTokenStream.Quotation, "\"abc \\f def\"");
            new JsonTokenStream("\"abc \\n def\"").ShouldYieldTokens(JsonTokenStream.Quotation, "\"abc \\n def\"");
            new JsonTokenStream("\"abc \\r def\"").ShouldYieldTokens(JsonTokenStream.Quotation, "\"abc \\r def\"");
            new JsonTokenStream("\"abc \\t def\"").ShouldYieldTokens(JsonTokenStream.Quotation, "\"abc \\t def\"");
            new JsonTokenStream("\"abc \\u005C def\"").ShouldYieldTokens(JsonTokenStream.Quotation, "\"abc \\u005C def\"");

            new JsonTokenStream("\" a \" \" b \" \" c \"").ShouldYieldTokens(JsonTokenStream.Quotation, "\" a \"", "\" b \"", "\" c \"");
        }

        [Fact]
        public void RecognizesNumbers()
        {
            new JsonTokenStream("0").ShouldYieldTokens(JsonTokenStream.Number, "0");
            new JsonTokenStream("1").ShouldYieldTokens(JsonTokenStream.Number, "1");
            new JsonTokenStream("12345").ShouldYieldTokens(JsonTokenStream.Number, "12345");
            new JsonTokenStream("12345").ShouldYieldTokens(JsonTokenStream.Number, "12345");
            new JsonTokenStream("0.012").ShouldYieldTokens(JsonTokenStream.Number, "0.012");
            new JsonTokenStream("0e1").ShouldYieldTokens(JsonTokenStream.Number, "0e1");
            new JsonTokenStream("0e+1").ShouldYieldTokens(JsonTokenStream.Number, "0e+1");
            new JsonTokenStream("0e-1").ShouldYieldTokens(JsonTokenStream.Number, "0e-1");
            new JsonTokenStream("0E1").ShouldYieldTokens(JsonTokenStream.Number, "0E1");
            new JsonTokenStream("0E+1").ShouldYieldTokens(JsonTokenStream.Number, "0E+1");
            new JsonTokenStream("0E-1").ShouldYieldTokens(JsonTokenStream.Number, "0E-1");
            new JsonTokenStream("10e11").ShouldYieldTokens(JsonTokenStream.Number, "10e11");
            new JsonTokenStream("10.123e11").ShouldYieldTokens(JsonTokenStream.Number, "10.123e11");
        }
    }
}