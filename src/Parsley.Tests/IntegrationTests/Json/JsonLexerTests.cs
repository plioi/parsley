namespace Parsley.Tests.IntegrationTests.Json;

using System.Collections.Generic;
using System.Linq;
using Shouldly;

public class JsonLexerTests
{
    static IEnumerable<Token> Tokenize(string input) => new JsonLexer().Tokenize(input);

    public void RecognizesSkippableWhitespace()
    {
        Tokenize(" ").ShouldBeEmpty();
        Tokenize("\t").ShouldBeEmpty();
        Tokenize("\n").ShouldBeEmpty();
        Tokenize("\r").ShouldBeEmpty();
        Tokenize(" \t\n\r").ShouldBeEmpty();
    }

    public void RecognizesKeywords()
    {
        Tokenize("null").Single().ShouldBe(JsonLexer.@null, "null");
        Tokenize("true").Single().ShouldBe(JsonLexer.@true, "true");
        Tokenize("false").Single().ShouldBe(JsonLexer.@false, "false");

        Tokenize("null true false")
            .ShouldList(t => t.ShouldBe(JsonLexer.@null, "null", 1, 1),
                t => t.ShouldBe(JsonLexer.@true, "true", 1, 6),
                t => t.ShouldBe(JsonLexer.@false, "false", 1, 11));
    }

    public void RecognizesOperators()
    {
        Tokenize(",").Single().ShouldBe(JsonLexer.Comma, ",");
        Tokenize("[").Single().ShouldBe(JsonLexer.OpenArray, "[");
        Tokenize("]").Single().ShouldBe(JsonLexer.CloseArray, "]");
        Tokenize("{").Single().ShouldBe(JsonLexer.OpenDictionary, "{");
        Tokenize("}").Single().ShouldBe(JsonLexer.CloseDictionary, "}");
        Tokenize(":").Single().ShouldBe(JsonLexer.Colon, ":");

        Tokenize(",[]{}:")
            .ShouldList(t => t.ShouldBe(JsonLexer.Comma, ",", 1, 1),
                t => t.ShouldBe(JsonLexer.OpenArray, "[", 1, 2),
                t => t.ShouldBe(JsonLexer.CloseArray, "]", 1, 3),
                t => t.ShouldBe(JsonLexer.OpenDictionary, "{", 1, 4),
                t => t.ShouldBe(JsonLexer.CloseDictionary, "}", 1, 5),
                t => t.ShouldBe(JsonLexer.Colon, ":", 1, 6));
    }

    public void RecognizesQuotations()
    {
        Tokenize("\"\"").Single().ShouldBe(JsonLexer.Quotation, "\"\"");
        Tokenize("\"a\"").Single().ShouldBe(JsonLexer.Quotation, "\"a\"");
        Tokenize("\"abc\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc\"");
        Tokenize("\"abc \\\" def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\\" def\"");
        Tokenize("\"abc \\\\ def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\\\ def\"");
        Tokenize("\"abc \\/ def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\/ def\"");
        Tokenize("\"abc \\b def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\b def\"");
        Tokenize("\"abc \\f def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\f def\"");
        Tokenize("\"abc \\n def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\n def\"");
        Tokenize("\"abc \\r def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\r def\"");
        Tokenize("\"abc \\t def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\t def\"");
        Tokenize("\"abc \\u005C def\"").Single().ShouldBe(JsonLexer.Quotation, "\"abc \\u005C def\"");

        Tokenize("\" a \" \" b \" \" c \"")
            .ShouldList(t => t.ShouldBe(JsonLexer.Quotation, "\" a \"", 1, 1),
                t => t.ShouldBe(JsonLexer.Quotation, "\" b \"", 1, 7),
                t => t.ShouldBe(JsonLexer.Quotation, "\" c \"", 1, 13));
    }

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
