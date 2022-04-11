using System.Text.RegularExpressions;

namespace Parsley.Tests;

class TokenKindTests
{
    readonly Pattern lower;
    readonly Pattern upper;
    readonly Pattern caseInsensitive;

    public TokenKindTests()
    {
        lower = new Pattern("Lowercase", @"[a-z]+");
        upper = new Pattern("Uppercase", @"[A-Z]+");
        caseInsensitive = new Pattern("Case Insensitive", @"[a-z]+", RegexOptions.IgnoreCase);
    }

    public void ProvidesConvenienceSubclassForRecognizingRegexPatterns()
    {
        lower.Name.ShouldBe("Lowercase");
        upper.Name.ShouldBe("Uppercase");
        caseInsensitive.Name.ShouldBe("Case Insensitive");

        upper.FailsToParse("abcDEF")
            .LeavingUnparsedInput("abcDEF")
            .WithMessage("(1, 1): Uppercase expected");

        lower.PartiallyParses("abcDEF")
            .LeavingUnparsedInput("DEF")
            .WithValue(token => token.ShouldBe(lower, "abc"));

        upper.Parses("DEF")
            .WithValue(token => token.ShouldBe(upper, "DEF"));

        caseInsensitive.Parses("abcDEF")
            .WithValue(token => token.ShouldBe(caseInsensitive, "abcDEF"));
    }

    public void UsesDescriptiveNameForToString()
    {
        lower.ToString().ShouldBe("Lowercase");
        upper.ToString().ShouldBe("Uppercase");
        caseInsensitive.ToString().ShouldBe("Case Insensitive");
    }

    public void ProvidesConvenienceSubclassForDefiningKeywords()
    {
        var foo = new Keyword("foo");

        foo.Name.ShouldBe("foo");

        foo.FailsToParse("bar")
            .LeavingUnparsedInput("bar")
            .WithMessage("(1, 1): foo expected");

        foo.Parses("foo")
            .WithValue(token => token.ShouldBe(foo, "foo"));

        foo.PartiallyParses("foo bar")
            .LeavingUnparsedInput(" bar")
            .WithValue(token => token.ShouldBe(foo, "foo"));

        foo.FailsToParse("foobar")
            .LeavingUnparsedInput("foobar")
            .WithMessage("(1, 1): foo expected");

        var notJustLetters = () => new Keyword(" oops ");
        notJustLetters.ShouldThrow<ArgumentException>("Keywords may only contain letters. (Parameter 'word')");
    }

    public void ProvidesConvenienceSubclassForDefiningOperators()
    {
        var star = new Operator("*");
        var doubleStar = new Operator("**");

        star.Name.ShouldBe("*");

        star.FailsToParse("a")
            .LeavingUnparsedInput("a")
            .WithMessage("(1, 1): * expected");

        star.Parses("*")
            .WithValue(token => token.ShouldBe(star, "*"));

        star.PartiallyParses("* *")
            .LeavingUnparsedInput(" *")
            .WithValue(token => token.ShouldBe(star, "*"));

        star.PartiallyParses("**")
            .LeavingUnparsedInput("*")
            .WithValue(token => token.ShouldBe(star, "*"));

        doubleStar.Name.ShouldBe("**");

        doubleStar.FailsToParse("a")
            .LeavingUnparsedInput("a")
            .WithMessage("(1, 1): ** expected");

        doubleStar.FailsToParse("*")
            .LeavingUnparsedInput("*")
            .WithMessage("(1, 1): ** expected");

        doubleStar.FailsToParse("* *")
            .LeavingUnparsedInput("* *")
            .WithMessage("(1, 1): ** expected");

        doubleStar.Parses("**")
            .WithValue(token => token.ShouldBe(doubleStar, "**"));

        doubleStar.PartiallyParses("***")
            .LeavingUnparsedInput("*")
            .WithValue(token => token.ShouldBe(doubleStar, "**"));
    }
}
