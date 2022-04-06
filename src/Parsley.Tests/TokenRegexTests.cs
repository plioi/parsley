﻿namespace Parsley.Tests
{
    using System.Text.RegularExpressions;
    using Shouldly;
    using Xunit;

    public class TokenRegexTests
    {
        [Fact]
        public void CanMatchRegexStartingFromGivenCharacterPosition()
        {
            var regex = new TokenRegex(@"[a-z]+");

            regex.Match("123abc0", 0).Success.ShouldBeFalse();

            regex.Match("123abc0", 3).Success.ShouldBeTrue();
            regex.Match("123abc0", 3).Value.ShouldBe("abc");

            regex.Match("123abc0", 4).Success.ShouldBeTrue();
            regex.Match("123abc0", 4).Value.ShouldBe("bc");
        }

        [Fact]
        public void CanMatchMultilineAndCommentedRegexes()
        {
            var regex = new TokenRegex(
                @"  [a-z]+   # Just Lower
                  | [A-Z]+   # Just Upper
                  | [0-9]+   # Just Digit");

            regex.Match("123Abc", 1).Value.ShouldBe("23");
            regex.Match("$23ab!", 0).Success.ShouldBeFalse();
            regex.Match("$23ab!", 1).Value.ShouldBe("23");
            regex.Match("$23ab!", 3).Value.ShouldBe("ab");
        }

        [Fact]
        public void SupportsOptionalRegexOptions()
        {
            var regex = new TokenRegex(@"[a-z]+", RegexOptions.IgnoreCase);

            regex.Match("123aBc0", 0).Success.ShouldBeFalse();

            regex.Match("123aBc0", 3).Success.ShouldBeTrue();
            regex.Match("123aBc0", 3).Value.ShouldBe("aBc");

            regex.Match("123aBc0", 4).Success.ShouldBeTrue();
            regex.Match("123aBc0", 4).Value.ShouldBe("Bc");
        }

        [Fact]
        public void HasStringRepresentation()
        {
            new TokenRegex(@"[a-zA-Z0-9]*").ToString().ShouldBe(@"[a-zA-Z0-9]*");
        }
    }
}