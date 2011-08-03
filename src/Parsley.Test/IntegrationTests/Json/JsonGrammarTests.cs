using System.Collections.Generic;
using NUnit.Framework;

namespace Parsley.Test.IntegrationTests.Json
{
    [TestFixture]
    public class JsonGrammarTests : JsonGrammar
    {
        [Test]
        public void ParsesTrueLiteral()
        {
            var tokens = new JsonLexer("true");

            JSON.Parses(tokens).IntoValue(value => ((bool)value).ShouldBeTrue());
        }

        [Test]
        public void ParsesFalseLiteral()
        {
            var tokens = new JsonLexer("false");

            JSON.Parses(tokens).IntoValue(value => ((bool)value).ShouldBeFalse());
        }

        [Test]
        public void ParsesNullLiteral()
        {
            var tokens = new JsonLexer("null");

            JSON.Parses(tokens).IntoValue(value => value.ShouldBeNull());
        }

        [Test]
        public void ParsesNumbers()
        {
            var tokens = new JsonLexer("10.123E-11");

            JSON.Parses(tokens).IntoValue(value => value.ShouldEqual(10.123E-11m));
        }

        [Test]
        public void ParsesQuotations()
        {
            var empty = new JsonLexer("\"\"");
            var filled = new JsonLexer("\"abc \\\" \\\\ \\/ \\b \\f \\n \\r \\t \\u263a def\"");
            const string expected = "abc \" \\ / \b \f \n \r \t ☺ def";

            JSON.Parses(empty).IntoValue(value => value.ShouldEqual(""));
            JSON.Parses(filled).IntoValue(value => value.ShouldEqual(expected));
        }

        [Test]
        public void ParsesArrays()
        {
            var empty = new JsonLexer("[]");
            var filled = new JsonLexer("[0, 1, 2]");

            JSON.Parses(empty).IntoValue(value => ((object[])value).ShouldBeEmpty());

            JSON.Parses(filled).IntoValue(value => value.ShouldEqual(new[] {0, 1, 2}));
        }

        [Test]
        public void ParsesDictionaries()
        {
            var empty = new JsonLexer("{}");
            var filled = new JsonLexer("{\"zero\" : 0, \"one\" : 1, \"two\" : 2}");

            JSON.Parses(empty).IntoValue(value => ((Dictionary<string, object>)value).Count.ShouldEqual(0));

            JSON.Parses(filled).IntoValue(value =>
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

                {""widget"": {
                    ""debug"": ""on"",
                    ""window"": {
                        ""title"": ""Sample Widget"",
                        ""name"": ""main_window"",
                        ""position"": [10, 20],
                        ""parent"": null,
                        ""maximized"": true,
                        ""transparent"": false
                    },
                    ""image"": { 
                        ""src"": ""Images/Sun.png"",
                        ""name"": ""sun"",
                        ""alignment"": ""center""
                    }
                }}

            ";

            JSON.Parses(new JsonLexer(complex)).IntoValue(value =>
            {
                dynamic json = value;

                ((string) json["widget"]["debug"]).ShouldEqual("on");

                ((string) json["widget"]["window"]["title"]).ShouldEqual("Sample Widget");
                ((string) json["widget"]["window"]["name"]).ShouldEqual("main_window");
                ((int) json["widget"]["window"]["position"][0]).ShouldEqual(10);
                ((int) json["widget"]["window"]["position"][1]).ShouldEqual(20);
                ((object) json["widget"]["window"]["parent"]).ShouldBeNull();
                ((bool) json["widget"]["window"]["maximized"]).ShouldBeTrue();
                ((bool) json["widget"]["window"]["transparent"]).ShouldBeFalse();

                ((string) json["widget"]["image"]["src"]).ShouldEqual("Images/Sun.png");
                ((string) json["widget"]["image"]["name"]).ShouldEqual("sun");
                ((string) json["widget"]["image"]["alignment"]).ShouldEqual("center");
            });
        }
    }
}