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

    public void ProvidesConvenienceSubclassForRecognizingNamedRegexPatterns()
    {
        lower.FailsToParse("ABCdef")
            .LeavingUnparsedInput("ABCdef")
            .WithMessage("(1, 1): Lowercase expected");

        upper.FailsToParse("abcDEF")
            .LeavingUnparsedInput("abcDEF")
            .WithMessage("(1, 1): Uppercase expected");

        caseInsensitive.FailsToParse("!abcDEF")
            .LeavingUnparsedInput("!abcDEF")
            .WithMessage("(1, 1): Case Insensitive expected");

        lower.PartiallyParses("abcDEF")
            .LeavingUnparsedInput("DEF")
            .WithValue(token => token.ShouldBe("abc"));

        upper.Parses("DEF")
            .WithValue(token => token.ShouldBe("DEF"));

        caseInsensitive.Parses("abcDEF")
            .WithValue(token => token.ShouldBe("abcDEF"));
    }

    public void ProvidesConvenienceSubclassForDefiningKeywords()
    {
        var foo = new Keyword("foo");

        foo.FailsToParse("bar")
            .LeavingUnparsedInput("bar")
            .WithMessage("(1, 1): foo expected");

        foo.Parses("foo")
            .WithValue(token => token.ShouldBe("foo"));

        foo.PartiallyParses("foo bar")
            .LeavingUnparsedInput(" bar")
            .WithValue(token => token.ShouldBe("foo"));

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

        star.FailsToParse("a")
            .LeavingUnparsedInput("a")
            .WithMessage("(1, 1): * expected");

        star.Parses("*")
            .WithValue(token => token.ShouldBe("*"));

        star.PartiallyParses("* *")
            .LeavingUnparsedInput(" *")
            .WithValue(token => token.ShouldBe("*"));

        star.PartiallyParses("**")
            .LeavingUnparsedInput("*")
            .WithValue(token => token.ShouldBe("*"));

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
            .WithValue(token => token.ShouldBe("**"));

        doubleStar.PartiallyParses("***")
            .LeavingUnparsedInput("*")
            .WithValue(token => token.ShouldBe("**"));
    }
}
