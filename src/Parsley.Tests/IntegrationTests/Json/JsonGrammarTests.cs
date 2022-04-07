namespace Parsley.Tests.IntegrationTests.Json;

class JsonGrammarTests : JsonGrammar
{
    static IEnumerable<Token> Tokenize(string input) => new JsonLexer().Tokenize(input);

    public void ParsesTrueLiteral()
    {
        var tokens = Tokenize("true");

        Json.Parses(tokens).WithValue(true);
    }

    public void ParsesFalseLiteral()
    {
        var tokens = Tokenize("false");

        Json.Parses(tokens).WithValue(false);
    }

    public void ParsesNullLiteral()
    {
        var tokens = Tokenize("null");

        Json.Parses(tokens).WithValue(value => value.ShouldBeNull());
    }

    public void ParsesNumbers()
    {
        var tokens = Tokenize("10.123E-11");

        Json.Parses(tokens).WithValue(10.123E-11m);
    }

    public void ParsesQuotations()
    {
        var empty = Tokenize("\"\"");
        var filled = Tokenize("\"abc \\\" \\\\ \\/ \\b \\f \\n \\r \\t \\u263a def\"");
        const string expected = "abc \" \\ / \b \f \n \r \t ☺ def";

        Json.Parses(empty).WithValue("");
        Json.Parses(filled).WithValue(expected);
    }

    public void ParsesArrays()
    {
        var empty = Tokenize("[]");
        var filled = Tokenize("[0, 1, 2]");

        Json.Parses(empty).WithValue(value => ((object[])value).ShouldBeEmpty());

        Json.Parses(filled).WithValue(value => value.ShouldBe(new[] { 0m, 1m, 2m }));
    }

    public void ParsesDictionaries()
    {
        var empty = Tokenize("{}");
        var filled = Tokenize("{\"zero\" : 0, \"one\" : 1, \"two\" : 2}");

        Json.Parses(empty).WithValue(value => ((Dictionary<string, object>)value).Count.ShouldBe(0));

        Json.Parses(filled).WithValue(value =>
        {
            var dictionary = (Dictionary<string, object>) value;
            dictionary["zero"].ShouldBe(0m);
            dictionary["one"].ShouldBe(1m);
            dictionary["two"].ShouldBe(2m);
        });
    }

    public void ParsesComplexJsonValues()
    {
        const string complex = @"

                {
                    ""numbers"" : [10, 20, 30],
                    ""window"":
                    {
                        ""title"": ""Sample Widget"",
                        ""parent"": null,
                        ""maximized"": true,
                        ""transparent"": false
                    }
                }

            ";

        var tokens = Tokenize(complex);

        Json.Parses(tokens).WithValue(value =>
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
        Json.FailsToParse(Tokenize("true $garbage$")).LeavingUnparsedInput("$garbage$").WithMessage("(1, 6): end of input expected");
        Json.FailsToParse(Tokenize("10.123E-11  $garbage$")).LeavingUnparsedInput("$garbage$").WithMessage("(1, 13): end of input expected");
        Json.FailsToParse(Tokenize("\"garbage\" $garbage$")).LeavingUnparsedInput("$garbage$").WithMessage("(1, 11): end of input expected");
        Json.FailsToParse(Tokenize("[0, 1, 2] $garbage$")).LeavingUnparsedInput("$garbage$").WithMessage("(1, 11): end of input expected");
        Json.FailsToParse(Tokenize("{\"zero\" : 0} $garbage$")).LeavingUnparsedInput("$garbage$").WithMessage("(1, 14): end of input expected");
    }
}
