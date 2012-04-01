using System.Collections.Generic;
using System.Linq;
using Should;
using Xunit;

namespace Parsley.IntegrationTests.Json
{
    public class JsonLexerTests
    {
        private static IEnumerable<Token> Tokenize(string input)
        {
            return new JsonLexer().Tokenize(new Text(input));
        }

        [Fact]
        public void RecognizesSkippableWhitespace()
        {
            Tokenize(" ").ShouldBeEmpty();
            Tokenize("\t").ShouldBeEmpty();
            Tokenize("\n").ShouldBeEmpty();
            Tokenize("\r").ShouldBeEmpty();
            Tokenize(" \t\n\r").ShouldBeEmpty();
        }

        [Fact]
        public void RecognizesKeywords()
        {
            Tokenize("null").Single().ShouldEqual(JsonLexer.@null, "null");
            Tokenize("true").Single().ShouldEqual(JsonLexer.@true, "true");
            Tokenize("false").Single().ShouldEqual(JsonLexer.@false, "false");

            Tokenize("null true false")
                .ShouldList(t => t.ShouldEqual(JsonLexer.@null, "null", 1, 1),
                            t => t.ShouldEqual(JsonLexer.@true, "true", 1, 6),
                            t => t.ShouldEqual(JsonLexer.@false, "false", 1, 11));
        }

        [Fact]
        public void RecognizesOperators()
        {
            Tokenize(",").Single().ShouldEqual(JsonLexer.Comma, ",");
            Tokenize("[").Single().ShouldEqual(JsonLexer.OpenArray, "[");
            Tokenize("]").Single().ShouldEqual(JsonLexer.CloseArray, "]");
            Tokenize("{").Single().ShouldEqual(JsonLexer.OpenDictionary, "{");
            Tokenize("}").Single().ShouldEqual(JsonLexer.CloseDictionary, "}");
            Tokenize(":").Single().ShouldEqual(JsonLexer.Colon, ":");

            Tokenize(",[]{}:")
                .ShouldList(t => t.ShouldEqual(JsonLexer.Comma, ",", 1, 1),
                            t => t.ShouldEqual(JsonLexer.OpenArray, "[", 1, 2),
                            t => t.ShouldEqual(JsonLexer.CloseArray, "]", 1, 3),
                            t => t.ShouldEqual(JsonLexer.OpenDictionary, "{", 1, 4),
                            t => t.ShouldEqual(JsonLexer.CloseDictionary, "}", 1, 5),
                            t => t.ShouldEqual(JsonLexer.Colon, ":", 1, 6));
        }

        [Fact]
        public void RecognizesQuotations()
        {
            Tokenize("\"\"").Single().ShouldEqual(JsonLexer.Quotation, "\"\"");
            Tokenize("\"a\"").Single().ShouldEqual(JsonLexer.Quotation, "\"a\"");
            Tokenize("\"abc\"").Single().ShouldEqual(JsonLexer.Quotation, "\"abc\"");
            Tokenize("\"abc \\\" def\"").Single().ShouldEqual(JsonLexer.Quotation, "\"abc \\\" def\"");
            Tokenize("\"abc \\\\ def\"").Single().ShouldEqual(JsonLexer.Quotation, "\"abc \\\\ def\"");
            Tokenize("\"abc \\/ def\"").Single().ShouldEqual(JsonLexer.Quotation, "\"abc \\/ def\"");
            Tokenize("\"abc \\b def\"").Single().ShouldEqual(JsonLexer.Quotation, "\"abc \\b def\"");
            Tokenize("\"abc \\f def\"").Single().ShouldEqual(JsonLexer.Quotation, "\"abc \\f def\"");
            Tokenize("\"abc \\n def\"").Single().ShouldEqual(JsonLexer.Quotation, "\"abc \\n def\"");
            Tokenize("\"abc \\r def\"").Single().ShouldEqual(JsonLexer.Quotation, "\"abc \\r def\"");
            Tokenize("\"abc \\t def\"").Single().ShouldEqual(JsonLexer.Quotation, "\"abc \\t def\"");
            Tokenize("\"abc \\u005C def\"").Single().ShouldEqual(JsonLexer.Quotation, "\"abc \\u005C def\"");

            Tokenize("\" a \" \" b \" \" c \"")
                .ShouldList(t => t.ShouldEqual(JsonLexer.Quotation, "\" a \"", 1, 1),
                            t => t.ShouldEqual(JsonLexer.Quotation, "\" b \"", 1, 7),
                            t => t.ShouldEqual(JsonLexer.Quotation, "\" c \"", 1, 13));
        }

        [Fact]
        public void RecognizesNumbers()
        {
            Tokenize("0").Single().ShouldEqual(JsonLexer.Number, "0");
            Tokenize("1").Single().ShouldEqual(JsonLexer.Number, "1");
            Tokenize("12345").Single().ShouldEqual(JsonLexer.Number, "12345");
            Tokenize("12345").Single().ShouldEqual(JsonLexer.Number, "12345");
            Tokenize("0.012").Single().ShouldEqual(JsonLexer.Number, "0.012");
            Tokenize("0e1").Single().ShouldEqual(JsonLexer.Number, "0e1");
            Tokenize("0e+1").Single().ShouldEqual(JsonLexer.Number, "0e+1");
            Tokenize("0e-1").Single().ShouldEqual(JsonLexer.Number, "0e-1");
            Tokenize("0E1").Single().ShouldEqual(JsonLexer.Number, "0E1");
            Tokenize("0E+1").Single().ShouldEqual(JsonLexer.Number, "0E+1");
            Tokenize("0E-1").Single().ShouldEqual(JsonLexer.Number, "0E-1");
            Tokenize("10e11").Single().ShouldEqual(JsonLexer.Number, "10e11");
            Tokenize("10.123e11").Single().ShouldEqual(JsonLexer.Number, "10.123e11");

            Tokenize("0 12 3e4 5E+67")
                .ShouldList(t => t.ShouldEqual(JsonLexer.Number, "0", 1, 1),
                            t => t.ShouldEqual(JsonLexer.Number, "12", 1, 3),
                            t => t.ShouldEqual(JsonLexer.Number, "3e4", 1, 6),
                            t => t.ShouldEqual(JsonLexer.Number, "5E+67", 1, 10));
        }
    }
}