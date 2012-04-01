using System.Collections.Generic;
using Should;
using Xunit;

namespace Parsley.IntegrationTests.Json
{
    public class JsonGrammarTests : JsonGrammar
    {
        private static IEnumerable<Token> Tokenize(string input)
        {
            return new JsonLexer().Tokenize(input);
        }

        [Fact]
        public void ParsesTrueLiteral()
        {
            var tokens = Tokenize("true");

            Json.Parses(tokens).IntoValue(value => ((bool)value).ShouldBeTrue());
        }

        [Fact]
        public void ParsesFalseLiteral()
        {
            var tokens = Tokenize("false");

            Json.Parses(tokens).IntoValue(value => ((bool)value).ShouldBeFalse());
        }

        [Fact]
        public void ParsesNullLiteral()
        {
            var tokens = Tokenize("null");

            Json.Parses(tokens).IntoValue(value => value.ShouldBeNull());
        }

        [Fact]
        public void ParsesNumbers()
        {
            var tokens = Tokenize("10.123E-11");

            Json.Parses(tokens).IntoValue(value => value.ShouldEqual(10.123E-11m));
        }

        [Fact]
        public void ParsesQuotations()
        {
            var empty = Tokenize("\"\"");
            var filled = Tokenize("\"abc \\\" \\\\ \\/ \\b \\f \\n \\r \\t \\u263a def\"");
            const string expected = "abc \" \\ / \b \f \n \r \t ☺ def";

            Json.Parses(empty).IntoValue(value => value.ShouldEqual(""));
            Json.Parses(filled).IntoValue(value => value.ShouldEqual(expected));
        }

        [Fact]
        public void ParsesArrays()
        {
            var empty = Tokenize("[]");
            var filled = Tokenize("[0, 1, 2]");

            Json.Parses(empty).IntoValue(value => ((object[])value).ShouldBeEmpty());

            Json.Parses(filled).IntoValue(value => value.ShouldEqual(new[] { 0m, 1m, 2m }));
        }

        [Fact]
        public void ParsesDictionaries()
        {
            var empty = Tokenize("{}");
            var filled = Tokenize("{\"zero\" : 0, \"one\" : 1, \"two\" : 2}");

            Json.Parses(empty).IntoValue(value => ((Dictionary<string, object>)value).Count.ShouldEqual(0));

            Json.Parses(filled).IntoValue(value =>
            {
                var dictionary = (Dictionary<string, object>) value;
                dictionary["zero"].ShouldEqual(0m);
                dictionary["one"].ShouldEqual(1m);
                dictionary["two"].ShouldEqual(2m);
            });
        }

        [Fact]
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

            Json.Parses(tokens).IntoValue(value =>
            {
                var json = (Dictionary<string, object>)value;
                json["numbers"].ShouldEqual(new[] {10m, 20m, 30m});

                var window = (Dictionary<string, object>) json["window"];
                window["title"].ShouldEqual("Sample Widget");
                window["parent"].ShouldBeNull();
                window["maximized"].ShouldEqual(true);
                window["transparent"].ShouldEqual(false);
            });
        }
    }
}