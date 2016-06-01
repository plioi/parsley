﻿using System;
using System.Text.RegularExpressions;
using Should;
using Xunit;

namespace Parsley
{
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
            token.ShouldEqual(lower, "abc", 1, 1);

            upper.TryMatch(abcDEF.Advance(3), out token).ShouldBeTrue();
            token.ShouldEqual(upper, "DEF", 1, 4);

            caseInsensitive.TryMatch(abcDEF, out token).ShouldBeTrue();
            token.ShouldEqual(caseInsensitive, "abcDEF", 1, 1);
        }

        [Fact]
        public void HasDescriptiveName()
        {
            lower.Name.ShouldEqual("Lowercase");
            upper.Name.ShouldEqual("Uppercase");
            caseInsensitive.Name.ShouldEqual("Case Insensitive");
        }

        [Fact]
        public void UsesDescriptiveNameForToString()
        {
            lower.ToString().ShouldEqual("Lowercase");
            upper.ToString().ShouldEqual("Uppercase");
            caseInsensitive.ToString().ShouldEqual("Case Insensitive");
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
            token.ShouldEqual(foo, "foo", 1, 1);

            foo.TryMatch(new Text("foo bar"), out token).ShouldBeTrue();
            token.ShouldEqual(foo, "foo", 1, 1);

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
            token.ShouldEqual(star, "*", 1, 1);

            star.TryMatch(new Text("* *"), out token).ShouldBeTrue();
            token.ShouldEqual(star, "*", 1, 1);

            star.TryMatch(new Text("**"), out token).ShouldBeTrue();
            token.ShouldEqual(star, "*", 1, 1);

            doubleStar.Name.ShouldEqual("**");

            doubleStar.TryMatch(new Text("a"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            doubleStar.TryMatch(new Text("*"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            doubleStar.TryMatch(new Text("* *"), out token).ShouldBeFalse();
            token.ShouldBeNull();

            doubleStar.TryMatch(new Text("**"), out token).ShouldBeTrue();
            token.ShouldEqual(doubleStar, "**", 1, 1);
        }

        [Fact]
        public void ProvidesConvenienceSubclassForTokensThatDoNotMatchLiteralsFromTheInput()
        {
            Token token;

            TokenKind.EndOfInput.ShouldBeType<Empty>();

            TokenKind.EndOfInput.Name.ShouldEqual("end of input");
            TokenKind.EndOfInput.Skippable.ShouldBeFalse();

            TokenKind.EndOfInput.TryMatch(new Text(""), out token).ShouldBeFalse();
            TokenKind.EndOfInput.TryMatch(new Text("foo"), out token).ShouldBeFalse();
        }
    }
}