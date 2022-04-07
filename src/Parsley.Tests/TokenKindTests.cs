namespace Parsley.Tests
{
    using System;
    using System.Text.RegularExpressions;
    using Shouldly;

    public class TokenKindTests
    {
        readonly TokenKind lower;
        readonly TokenKind upper;
        readonly TokenKind caseInsensitive;
        readonly Text abcDEF;

        public TokenKindTests()
        {
            lower = new Pattern("Lowercase", @"[a-z]+");
            upper = new Pattern("Uppercase", @"[A-Z]+");
            caseInsensitive = new Pattern("Case Insensitive", @"[a-z]+", RegexOptions.IgnoreCase);
            abcDEF = new Text("abcDEF");
        }

        public void ProducesNullTokenUponFailedMatch()
        {
            upper.TryMatch(abcDEF, out var token).ShouldBeFalse();
            token.ShouldBeNull();
        }

        public void ProducesTokenUponSuccessfulMatch()
        {
            lower.TryMatch(abcDEF, out var token).ShouldBeTrue();
            token.ShouldBe(lower, "abc", 1, 1);

            upper.TryMatch(abcDEF.Advance(3), out token).ShouldBeTrue();
            token.ShouldBe(upper, "DEF", 1, 4);

            caseInsensitive.TryMatch(abcDEF, out token).ShouldBeTrue();
            token.ShouldBe(caseInsensitive, "abcDEF", 1, 1);
        }

        public void HasDescriptiveName()
        {
            lower.Name.ShouldBe("Lowercase");
            upper.Name.ShouldBe("Uppercase");
            caseInsensitive.Name.ShouldBe("Case Insensitive");
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

            foo.TryMatch(new Text("bar"), out var token).ShouldBeFalse();
            token.ShouldBeNull();

            foo.TryMatch(new Text("foo"), out token).ShouldBeTrue();
            token.ShouldBe(foo, "foo", 1, 1);

            foo.TryMatch(new Text("foo bar"), out token).ShouldBeTrue();
            token.ShouldBe(foo, "foo", 1, 1);

            foo.TryMatch(new Text("foobar"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            var notJustLetters = () => new Keyword(" oops ");
            notJustLetters.ShouldThrow<ArgumentException>("Keywords may only contain letters.\r\nParameter name: word");
        }

        public void ProvidesConvenienceSubclassForDefiningOperators()
        {
            var star = new Operator("*");
            var doubleStar = new Operator("**");

            star.Name.ShouldBe("*");

            star.TryMatch(new Text("a"), out var token).ShouldBeFalse();
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

        public void ProvidesConvenienceSubclassForTokensThatDoNotMatchLiteralsFromTheInput()
        {
            TokenKind.EndOfInput.ShouldBeOfType<Empty>();

            TokenKind.EndOfInput.Name.ShouldBe("end of input");
            TokenKind.EndOfInput.Skippable.ShouldBeFalse();

            TokenKind.EndOfInput.TryMatch(new Text(""), out _).ShouldBeFalse();
            TokenKind.EndOfInput.TryMatch(new Text("foo"), out _).ShouldBeFalse();
        }
    }
}
