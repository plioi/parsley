using System.IO;

namespace Parsley.Tests.IntegrationTests.Json
{
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using Xunit;

    public class JsonLinedLexerTests
    {
        static IEnumerable<Token> Tokenize(string input) => new JsonLinedLexer().Tokenize(new StringReader(input));

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
            Tokenize("null").Single().ShouldBe(JsonLexer.Null, "null");
            Tokenize("true").Single().ShouldBe(JsonLexer.True, "true");
            Tokenize("false").Single().ShouldBe(JsonLexer.False, "false");

            Tokenize("null \n true \nfalse")
                .ShouldList(t => t.ShouldBe(JsonLexer.Null, "null", 1, 1),
                    t => t.ShouldBe(JsonLexer.True, "true", 2, 2),
                    t => t.ShouldBe(JsonLexer.False, "false", 3, 1));
        }

        [Fact]
        public void RecognizesOperators()
        {
            Tokenize(",").Single().ShouldBe(JsonLexer.Comma, ",");
            Tokenize("[").Single().ShouldBe(JsonLexer.OpenArray, "[");
            Tokenize("]").Single().ShouldBe(JsonLexer.CloseArray, "]");
            Tokenize("{").Single().ShouldBe(JsonLexer.OpenDictionary, "{");
            Tokenize("}").Single().ShouldBe(JsonLexer.CloseDictionary, "}");
            Tokenize(":").Single().ShouldBe(JsonLexer.Colon, ":");

            Tokenize(",[\n]{}:\n\n")
                .ShouldList(t => t.ShouldBe(JsonLexer.Comma, ",", 1, 1),
                    t => t.ShouldBe(JsonLexer.OpenArray, "[", 1, 2),
                    t => t.ShouldBe(JsonLexer.CloseArray, "]", 2, 1),
                    t => t.ShouldBe(JsonLexer.OpenDictionary, "{", 2, 2),
                    t => t.ShouldBe(JsonLexer.CloseDictionary, "}", 2, 3),
                    t => t.ShouldBe(JsonLexer.Colon, ":", 2, 4));
        }

        [Fact]
        public void RecognizesQuotations()
        {
            Tokenize("\n\"\"").Single().ShouldBe(JsonLexer.Quotation, "\"\"");
            Tokenize("\"a\"").Single().ShouldBe(JsonLexer.Quotation, "\"a\"");
            Tokenize("\"abc\"\n").Single().ShouldBe(JsonLexer.Quotation, "\"abc\"");
            Tokenize("\n\n\"abc \\\" def\"\n\n").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\\" def\"");
            Tokenize("\"abc \\\\ def\"\n\n\n\n").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\\\ def\"");
            Tokenize("\n\n\"abc \\/ def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\/ def\"");
            Tokenize("\n\n\"abc \\b def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\b def\"");
            Tokenize("\n\n\"abc \\f def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\f def\"");
            Tokenize("\n\n\"abc \\n def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\n def\"");
            Tokenize("\n\n\n\n\"abc \\r def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\r def\"");
            Tokenize("\n\n\"abc \\t def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\t def\"");
            Tokenize("\"abc \\u005C def\"\n\n").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\u005C def\"");

            Tokenize("\" a \" \n \" b \" \n \" c \"")
                .ShouldList(t => t.ShouldBe(JsonLexer.Quotation, "\" a \"", 1, 1),
                    t => t.ShouldBe(JsonLexer.Quotation, "\" b \"", 2, 2),
                    t => t.ShouldBe(JsonLexer.Quotation, "\" c \"", 3, 2));
        }

        [Fact]
        public void RecognizesNumbers()
        {
            Tokenize("0").Single().ShouldBe(JsonLexer.Number, "0");
            Tokenize("1").Single().ShouldBe(JsonLexer.Number, "1");
            Tokenize("12345").Single().ShouldBe(JsonLexer.Number, "12345");
            Tokenize("12345").Single().ShouldBe(JsonLexer.Number, "12345");
            Tokenize("0.012").Single().ShouldBe(JsonLexer.Number, "0.012");
            Tokenize("0e1").Single().ShouldBe(JsonLexer.Number, "0e1");
            Tokenize("0e+1").Single().ShouldBe(JsonLexer.Number, "0e+1");
            Tokenize("0e-1").Single().ShouldBe(JsonLexer.Number, "0e-1");
            Tokenize("0E1").Single().ShouldBe(JsonLexer.Number, "0E1");
            Tokenize("0E+1").Single().ShouldBe(JsonLexer.Number, "0E+1");
            Tokenize("0E-1").Single().ShouldBe(JsonLexer.Number, "0E-1");
            Tokenize("10e11").Single().ShouldBe(JsonLexer.Number, "10e11");
            Tokenize("10.123e11").Single().ShouldBe(JsonLexer.Number, "10.123e11");

            Tokenize("0 12 3e4 5E+67")
                .ShouldList(t => t.ShouldBe(JsonLexer.Number, "0", 1, 1),
                    t => t.ShouldBe(JsonLexer.Number, "12", 1, 3),
                    t => t.ShouldBe(JsonLexer.Number, "3e4", 1, 6),
                    t => t.ShouldBe(JsonLexer.Number, "5E+67", 1, 10));
        }
    }
}