using Parsley.Tests.Fixtures;

namespace Parsley.Tests
{
    using System;
    using System.Text.RegularExpressions;
    using Shouldly;
    using Xunit;

    public class TokenKindTests
    {
        readonly TokenKind lower;
        readonly TokenKind upper;
        readonly TokenKind caseInsensitive;
        readonly TextTestFixture abcDEF;

        public TokenKindTests()
        {
            lower = new Pattern("Lowercase", @"[a-z]+");
            upper = new Pattern("Uppercase", @"[A-Z]+");
            caseInsensitive = new Pattern("Case Insensitive", @"[a-z]+", RegexOptions.IgnoreCase);
            abcDEF = new TextTestFixture("abcDEF");
        }

        [Fact]
        public void ProducesNullTokenUponFailedMatch()
        {
            Token token;

            upper.TryMatch((Text)abcDEF, out token).ShouldBeFalse();
            token.ShouldBeNull();
        }

        [Fact]
        public void ProducesTokenUponSuccessfulMatch()
        {
            Token token;

            lower.TryMatch((Text)abcDEF, out token).ShouldBeTrue();
            token.ShouldBe(lower, "abc", 1, 1);

            upper.TryMatch(abcDEF.Advance(3), out token).ShouldBeTrue();
            token.ShouldBe(upper, "DEF", 1, 4);

            caseInsensitive.TryMatch((Text)abcDEF, out token).ShouldBeTrue();
            token.ShouldBe(caseInsensitive, "abcDEF", 1, 1);
        }

        [Fact]
        public void HasDescriptiveName()
        {
            lower.Name.ShouldBe("Lowercase");
            upper.Name.ShouldBe("Uppercase");
            caseInsensitive.Name.ShouldBe("Case Insensitive");
        }

        [Fact]
        public void UsesDescriptiveNameForToString()
        {
            lower.ToString().ShouldBe("Lowercase");
            upper.ToString().ShouldBe("Uppercase");
            caseInsensitive.ToString().ShouldBe("Case Insensitive");
        }

        [Fact]
        public void ProvidesConvenienceSubclassForDefiningKeywords()
        {
            Token token;
            var foo = new Keyword("foo");

            foo.Name.ShouldBe("foo");

            foo.TryMatch(new Text("bar"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            foo.TryMatch(new Text("foo"), out token).ShouldBeTrue();
            token.ShouldBe(foo, "foo", 1, 1);

            foo.TryMatch(new Text("foo bar"), out token).ShouldBeTrue();
            token.ShouldBe(foo, "foo", 1, 1);

            foo.TryMatch(new Text("foobar"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            Action notJustLetters = () => new Keyword(" oops ");

            notJustLetters.ShouldThrow<ArgumentException>("Keywords may only contain letters.\r\nParameter name: keyword");
        }

        [Fact]
        public void ProvidesConvenienceSubclassForDefiningOperators()
        {
            Token token;
            var star = new Operator("*");
            var doubleStar = new Operator("**");

            star.Name.ShouldBe("*");

            star.TryMatch(new Text("a"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            star.TryMatch(new Text("*"), out token).ShouldBeTrue();
            token.ShouldBe(star, "*", 1, 1);

            star.TryMatch(new Text("* *"), out token).ShouldBeTrue();
            token.ShouldBe(star, "*", 1, 1);

            star.TryMatch(new Text("**"), out token).ShouldBeTrue();
            token.ShouldBe(star, "*", 1, 1);

            doubleStar.Name.ShouldBe("**");

            doubleStar.TryMatch(new Text("a"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            doubleStar.TryMatch(new Text("*"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            doubleStar.TryMatch(new Text("* *"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            doubleStar.TryMatch(new Text("**"), out token).ShouldBeTrue();
            token.ShouldBe(doubleStar, "**", 1, 1);
        }

        [Fact]
        public void ProvidesConvenienceSubclassForTokensThatDoNotMatchLiteralsFromTheInput()
        {
            Token token;

            TokenKind.EndOfInput.ShouldBeOfType<Empty>();

            TokenKind.EndOfInput.Name.ShouldBe("end of input");
            TokenKind.EndOfInput.Skippable.ShouldBeFalse();

            TokenKind.EndOfInput.TryMatch(new Text(""), out token).ShouldBeFalse();
            TokenKind.EndOfInput.TryMatch(new Text("foo"), out token).ShouldBeFalse();
        }
    }
}