namespace Parsley.Tests.IntegrationTests.Json;

using static JsonGrammar;

class JsonGrammarTests
{
    public void ParsesTrueLiteral()
    {
        JsonDocument.Parses("true").WithValue(true);
    }

    public void ParsesFalseLiteral()
    {
        JsonDocument.Parses("false").WithValue(false);
    }

    public void ParsesNullLiteral()
    {
        JsonDocument.Parses("null").WithValue((object)null);
    }

    public void ParsesNumbers()
    {
        JsonDocument.Parses("0").WithValue(0m);
        JsonDocument.Parses("12345").WithValue(12345m);
        JsonDocument.Parses("0.012").WithValue(0.012m);
        JsonDocument.Parses("0e1").WithValue(0e1m);
        JsonDocument.Parses("0e+1").WithValue(0e+1m);
        JsonDocument.Parses("0e-1").WithValue(0e-1m);
        JsonDocument.Parses("0E1").WithValue(0E1m);
        JsonDocument.Parses("0E+1").WithValue(0E+1m);
        JsonDocument.Parses("0E-1").WithValue(0E-1m);
        JsonDocument.Parses("10e11").WithValue(10e11m);
        JsonDocument.Parses("10.123e11").WithValue(10.123e11m);
        JsonDocument.Parses("10.123E-11").WithValue(10.123E-11m);
    }

    public void ParsesQuotations()
    {
        var empty = "\"\"";
        var filled = "\"abc \\\" \\\\ \\/ \\b \\f \\n \\r \\t \\u263a def\"";
        const string expected = "abc \" \\ / \b \f \n \r \t ☺ def";

        JsonDocument.Parses(empty).WithValue("");
        JsonDocument.Parses(filled).WithValue(expected);
    }

    public void ParsesArrays()
    {
        var empty = "[]";
        var filled = "[0, 1, 2]";

        JsonDocument.Parses(empty).WithValue(value => ((object[])value).ShouldBeEmpty());

        JsonDocument.Parses(filled).WithValue(value => value.ShouldBe(new[] { 0m, 1m, 2m }));
    }

    public void ParsesDictionaries()
    {
        var empty = "{}";
        var filled = "{\"zero\" : 0, \"one\" : 1, \"two\" : 2}";

        JsonDocument.Parses(empty).WithValue(value => ((Dictionary<string, object>)value).Count.ShouldBe(0));

        JsonDocument.Parses(filled).WithValue(value =>
        {
            var dictionary = (Dictionary<string, object>) value;
            dictionary["zero"].ShouldBe(0m);
            dictionary["one"].ShouldBe(1m);
            dictionary["two"].ShouldBe(2m);
        });
    }

    public void ParsesComplexJsonValuesSkippingOptionalWhitespace()
    {
        const string whitespaceCharacters = "\r\n\t";
        const string complex = whitespaceCharacters + @"

                {
                    ""numbers"" : [ 10, 20, 30 ],
                    ""window"":
                    {
                        ""title"": ""Sample Widget""," + whitespaceCharacters + @"
                        ""parent"": null,
                        ""maximized"": true  ,
                        ""transparent"": false
                    }
                }

            " + whitespaceCharacters;

        JsonDocument.Parses(complex).WithValue(value =>
        {
            var json = (Dictionary<string, object>)value;
            json["numbers"].ShouldBe(new[] {10m, 20m, 30m});

            var window = (Dictionary<string, object>) json["window"];
            window["title"].ShouldBe("Sample Widget");
            window["parent"].ShouldBeNull();
            window["maximized"].ShouldBe(true);
            window["transparent"].ShouldBe(false);
        });
    }

    public void RequiresEndOfInputAfterFirstValidJsonValue()
    {
        JsonDocument.FailsToParse("true $garbage$", "$garbage$").WithMessage("(1, 6): end of input expected");
        JsonDocument.FailsToParse("10.123E-11  $garbage$", "$garbage$").WithMessage("(1, 13): end of input expected");
        JsonDocument.FailsToParse("\"garbage\" $garbage$", "$garbage$").WithMessage("(1, 11): end of input expected");
        JsonDocument.FailsToParse("[0, 1, 2] $garbage$", "$garbage$").WithMessage("(1, 11): end of input expected");
        JsonDocument.FailsToParse("{\"zero\" : 0} $garbage$", "$garbage$").WithMessage("(1, 14): end of input expected");
    }
}
