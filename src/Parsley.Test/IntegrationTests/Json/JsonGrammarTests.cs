using System.Collections.Generic;
using NUnit.Framework;

namespace Parsley.IntegrationTests.Json
{
    [TestFixture]
    public class JsonGrammarTests : JsonGrammar
    {
        [Test]
        public void ParsesTrueLiteral()
        {
            var tokens = new JsonLexer("true");

            Json.Parses(tokens).IntoValue(value => ((bool)value).ShouldBeTrue());
        }

        [Test]
        public void ParsesFalseLiteral()
        {
            var tokens = new JsonLexer("false");

            Json.Parses(tokens).IntoValue(value => ((bool)value).ShouldBeFalse());
        }

        [Test]
        public void ParsesNullLiteral()
        {
            var tokens = new JsonLexer("null");

            Json.Parses(tokens).IntoValue(value => value.ShouldBeNull());
        }

        [Test]
        public void ParsesNumbers()
        {
            var tokens = new JsonLexer("10.123E-11");

            Json.Parses(tokens).IntoValue(value => value.ShouldEqual(10.123E-11m));
        }

        [Test]
        public void ParsesQuotations()
        {
            var empty = new JsonLexer("\"\"");
            var filled = new JsonLexer("\"abc \\\" \\\\ \\/ \\b \\f \\n \\r \\t \\u263a def\"");
            const string expected = "abc \" \\ / \b \f \n \r \t ☺ def";

            Json.Parses(empty).IntoValue(value => value.ShouldEqual(""));
            Json.Parses(filled).IntoValue(value => value.ShouldEqual(expected));
        }

        [Test]
        public void ParsesArrays()
        {
            var empty = new JsonLexer("[]");
            var filled = new JsonLexer("[0, 1, 2]");

            Json.Parses(empty).IntoValue(value => ((object[])value).ShouldBeEmpty());

            Json.Parses(filled).IntoValue(value => value.ShouldEqual(new[] { 0, 1, 2 }));
        }

        [Test]
        public void ParsesDictionaries()
        {
            var empty = new JsonLexer("{}");
            var filled = new JsonLexer("{\"zero\" : 0, \"one\" : 1, \"two\" : 2}");

            Json.Parses(empty).IntoValue(value => ((Dictionary<string, object>)value).Count.ShouldEqual(0));

            Json.Parses(filled).IntoValue(value =>
            {
                var dictionary = (Dictionary<string, object>) value;
                dictionary["zero"].ShouldEqual(0);
                dictionary["one"].ShouldEqual(1);
                dictionary["two"].ShouldEqual(2);
            });
        }

        [Test]
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

            var jsonLexer = new JsonLexer(complex);

            Json.Parses(jsonLexer).IntoValue(value =>
            {
                var json = (Dictionary<string, object>)value;
                json["numbers"].ShouldEqual(new[] {10, 20, 30});

                var window = (Dictionary<string, object>) json["window"];
                window["title"].ShouldEqual("Sample Widget");
                window["parent"].ShouldBeNull();
                window["maximized"].ShouldEqual(true);
                window["transparent"].ShouldEqual(false);
            });
        }
    }
}