namespace Parsley.Tests.IntegrationTests.Json;

using static JsonGrammar;

class JsonGrammarTests
{
    public void ParsesTrueLiteral()
    {
        JsonDocument.Parses("true").ShouldBe(true);
    }

    public void ParsesFalseLiteral()
    {
        JsonDocument.Parses("false").ShouldBe(false);
    }

    public void ParsesNullLiteral()
    {
        JsonDocument.Parses("null").ShouldBe(null);
    }

    public void ParsesNumbers()
    {
        JsonDocument.Parses("0").ShouldBe(0m);
        JsonDocument.Parses("12345").ShouldBe(12345m);
        JsonDocument.Parses("0.012").ShouldBe(0.012m);
        JsonDocument.Parses("0e1").ShouldBe(0e1m);
        JsonDocument.Parses("0e+1").ShouldBe(0e+1m);
        JsonDocument.Parses("0e-1").ShouldBe(0e-1m);
        JsonDocument.Parses("0E1").ShouldBe(0E1m);
        JsonDocument.Parses("0E+1").ShouldBe(0E+1m);
        JsonDocument.Parses("0E-1").ShouldBe(0E-1m);
        JsonDocument.Parses("10e11").ShouldBe(10e11m);
        JsonDocument.Parses("10.123e11").ShouldBe(10.123e11m);
        JsonDocument.Parses("10.123E-11").ShouldBe(10.123E-11m);
        JsonDocument.FailsToParse("9" + decimal.MaxValue, "", "decimal within valid range expected");
    }

    public void ParsesQuotations()
    {
        var empty = "\"\"";
        var filled = "\"abc \\\" \\\\ \\/ \\b \\f \\n \\r \\t \\u263a def\"";
        const string expected = "abc \" \\ / \b \f \n \r \t ☺ def";

        JsonDocument.Parses(empty).ShouldBe("");
        JsonDocument.Parses(filled).ShouldBe(expected);
    }

    public void ParsesArrays()
    {
        var empty = "[]";
        var filled = "[0, 1, 2]";

        var emptyValue = JsonDocument.Parses(empty);
        emptyValue.ShouldNotBeNull();
        ((object[]) emptyValue).ShouldBeEmpty();

        var value = JsonDocument.Parses(filled);
        value.ShouldNotBeNull();
        ((object[]) value).ShouldBe(new object[] { 0m, 1m, 2m });
    }

    public void ParsesDictionaries()
    {
        var empty = "{}";
        var filled = "{\"zero\" : 0, \"one\" : 1, \"two\" : 2}";

        var emptyValue = JsonDocument.Parses(empty);
        emptyValue.ShouldNotBeNull();
        ((Dictionary<string, object>) emptyValue).Count.ShouldBe(0);

        var value = JsonDocument.Parses(filled);
        value.ShouldNotBeNull();

        var dictionary = (Dictionary<string, object>) value;
        dictionary.Count.ShouldBe(3);
        dictionary["zero"].ShouldBe(0m);
        dictionary["one"].ShouldBe(1m);
        dictionary["two"].ShouldBe(2m);
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

        var value = JsonDocument.Parses(complex);
        value.ShouldNotBeNull();

        var json = (Dictionary<string, object>) value;
        json["numbers"].ShouldBe(new[] { 10m, 20m, 30m });

        var window = (Dictionary<string, object>) json["window"];
        window["title"].ShouldBe("Sample Widget");
        window["parent"].ShouldBeNull();
        window["maximized"].ShouldBe(true);
        window["transparent"].ShouldBe(false);
    }

    public void ProvidesUsefulErrorMessagesForDeeplyPlacedTokenizerErrors()
    {
        const string whitespaceCharacters = "\r\n\t";
        const string invalidSlashP = whitespaceCharacters + @"

                {
                    ""numbers"" : [ 10, 20, 30 ],
                    ""window"":
                    {
                        ""title"": ""Sample Widget""," + whitespaceCharacters + @"
                        ""parent"": null,
                        ""maximized"": true  ,
                        ""trans\parent"": false
                    }
                }";

        JsonDocument.FailsToParse(invalidSlashP,
            @"parent"": false
                    }
                }",
            "(escape character or unicode escape sequence) expected");
    }

    public void ProvidesUsefulErrorMessagesForDeeplyPlacedGrammarErrors()
    {
        const string whitespaceCharacters = "\r\n\t";
        const string invalidSlashP = whitespaceCharacters + @"

                {
                    ""numbers"" : [ 10, 20, 30 ],
                    ""window"":
                    {
                        ""title"": ""Sample Widget""," + whitespaceCharacters + @"
                        ""parent"": null,
                        ""maximized"": true  ,
                        ""transparent"": false 7
                    }
                }";

        JsonDocument.FailsToParse(invalidSlashP,
            @"7
                    }
                }",
            "} expected");
    }
}
