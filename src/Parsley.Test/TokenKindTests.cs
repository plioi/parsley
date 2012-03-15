using System;
using Should;
using Xunit;

namespace Parsley
{
    public class TokenKindTests
    {
        private readonly TokenKind lower;
        private readonly TokenKind upper;
        private readonly Text abcDEF;

        public TokenKindTests()
        {
            lower = new Pattern("Lowercase", @"[a-z]+");
            upper = new Pattern("Uppercase", @"[A-Z]+");
            abcDEF = new Text("abcDEF");
        }

        [Fact]
        public void ProducesNullTokenUponFailedMatch()
        {
            Token token;

            upper.TryMatch(abcDEF, out token).ShouldBeFalse();
            token.ShouldBeNull();
        }

        [Fact]
        public void ProducesTokenUponSuccessfulMatch()
        {
            Token token;

            lower.TryMatch(abcDEF, out token).ShouldBeTrue();
            token.ShouldBe(lower, "abc", 1, 1);

            upper.TryMatch(abcDEF.Advance(3), out token).ShouldBeTrue();
            token.ShouldBe(upper, "DEF", 1, 4);
        }

        [Fact]
        public void HasDescriptiveName()
        {
            lower.Name.ShouldEqual("Lowercase");
            upper.Name.ShouldEqual("Uppercase");
        }

        [Fact]
        public void UsesDescriptiveNameForToString()
        {
            lower.ToString().ShouldEqual("Lowercase");
            upper.ToString().ShouldEqual("Uppercase");
        }

        [Fact]
        public void ProvidesConvenienceSubclassForDefiningKeywords()
        {
            Token token;
            var foo = new Keyword("foo");

            foo.Name.ShouldEqual("foo");

            foo.TryMatch(new Text("bar"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            foo.TryMatch(new Text("foo"), out token).ShouldBeTrue();
            token.ShouldBe(foo, "foo", 1, 1);

            foo.TryMatch(new Text("foo bar"), out token).ShouldBeTrue();
            token.ShouldBe(foo, "foo", 1, 1);

            foo.TryMatch(new Text("foobar"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            Action notJustLetters = () => new Keyword(" oops ");
            notJustLetters.ShouldThrow<ArgumentException>("Keywords may only contain letters.\r\nParameter name: word");
        }

        [Fact]
        public void ProvidesConvenienceSubclassForDefiningOperators()
        {
            Token token;
            var star = new Operator("*");
            var doubleStar = new Operator("**");

            star.Name.ShouldEqual("*");

            star.TryMatch(new Text("a"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            star.TryMatch(new Text("*"), out token).ShouldBeTrue();
            token.ShouldBe(star, "*", 1, 1);

            star.TryMatch(new Text("* *"), out token).ShouldBeTrue();
            token.ShouldBe(star, "*", 1, 1);

            star.TryMatch(new Text("**"), out token).ShouldBeTrue();
            token.ShouldBe(star, "*", 1, 1);

            doubleStar.Name.ShouldEqual("**");

            doubleStar.TryMatch(new Text("a"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            doubleStar.TryMatch(new Text("*"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            doubleStar.TryMatch(new Text("* *"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            doubleStar.TryMatch(new Text("**"), out token).ShouldBeTrue();
            token.ShouldBe(doubleStar, "**", 1, 1);
        }
    }
}