﻿using Shouldly;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Parsley.Tests.IntegrationTests.Json
{
    public class JsonGrammarTests
    {
        static IEnumerable<Token> Tokenize(string input) => new JsonLexer().Tokenize(input);
        static IEnumerable<Token> TokenizeLined(string input) => new JsonLinedLexer().Tokenize(new StringReader(input));
        static readonly JsonGrammar Json = new JsonGrammar();

        [Fact]
        public void ParsesTrueLiteral()
        {
            var tokens = Tokenize("true");

            Json.Parses(tokens).WithValue(true);
        }

        [Fact]
        public void ParsesFalseLiteral()
        {
            var tokens = Tokenize("false");

            Json.Parses(tokens).WithValue(false);
        }

        [Fact]
        public void ParsesNullLiteral()
        {
            var tokens = Tokenize("null");

            Json.Parses(tokens).WithValue(value => value.ShouldBeNull());
        }

        [Fact]
        public void ParsesNumbers()
        {
            var tokens = Tokenize("10.123E-11");

            Json.Parses(tokens).WithValue(10.123E-11m);
        }

        [Fact]
        public void ParsesQuotations()
        {
            var empty = Tokenize("\"\"");
            var filled = Tokenize("\"abc \\\" \\\\ \\/ \\b \\f \\n \\r \\t \\u263a def\"");
            const string expected = "abc \" \\ / \b \f \n \r \t ☺ def";

            Json.Parses(empty).WithValue("");
            Json.Parses(filled).WithValue(expected);
        }

        [Fact]
        public void ParsesArrays()
        {
            var empty = Tokenize("[]");
            var filled = Tokenize("[0, 1, 2]");

            Json.Parses(empty).WithValue(value => ((IList<object>)value).ShouldBeEmpty());

            Json.Parses(filled).WithValue(value => value.ShouldBe(new[] { 0m, 1m, 2m }));
        }

        [Fact]
        public void ParsesDictionaries()
        {
            var empty = Tokenize("{}");

            Json.Parses(empty).WithValue(value => ((Dictionary<string, object>)value).Count.ShouldBe(0));

            var filled = "{\"zero\" \n : \n 0, \"one\" \n \n\n: 1, \"two\" \n\n : 2}";

            foreach (var tokens in new[] { Tokenize(filled), TokenizeLined(filled) })
                Json.Parses(tokens).WithValue(value =>
                {
                    var dictionary = (Dictionary<string, object>) value;
                    dictionary["zero"].ShouldBe(0m);
                    dictionary["one"].ShouldBe(1m);
                    dictionary["two"].ShouldBe(2m);
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

            foreach(var tokens in new []{ Tokenize(complex), TokenizeLined(complex) })
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

        [Fact]
        public void RequiresEndOfInputAfterFirstValidJsonValue()
        {
            Json.FailsToParse(Tokenize("true $garbage$")).LeavingUnparsedTokens("$garbage$").WithMessage("(1, 6): end of input expected");
            Json.FailsToParse(Tokenize("10.123E-11  $garbage$")).LeavingUnparsedTokens("$garbage$").WithMessage("(1, 13): end of input expected");
            Json.FailsToParse(Tokenize("\"garbage\" $garbage$")).LeavingUnparsedTokens("$garbage$").WithMessage("(1, 11): end of input expected");
            Json.FailsToParse(Tokenize("[0, 1, 2] $garbage$")).LeavingUnparsedTokens("$garbage$").WithMessage("(1, 11): end of input expected");
            Json.FailsToParse(Tokenize("{\"zero\" : 0} $garbage$")).LeavingUnparsedTokens("$garbage$").WithMessage("(1, 14): end of input expected");
        }
    }
}