using Should;
using Xunit;

namespace Parsley
{
    public class TokenRegexTests
    {
        [Fact]
        public void CanMatchRegexStartingFromGivenCharacterPosition()
        {
            var regex = new TokenRegex(@"[a-z]+");

            regex.Match("123abc0", 0).Success.ShouldBeFalse();

            regex.Match("123abc0", 3).Success.ShouldBeTrue();
            regex.Match("123abc0", 3).Value.ShouldEqual("abc");

            regex.Match("123abc0", 4).Success.ShouldBeTrue();
            regex.Match("123abc0", 4).Value.ShouldEqual("bc");
        }

        [Fact]
        public void CanMatchMultilineAndCommentedRegexes()
        {
            var regex = new TokenRegex(
                @"  [a-z]+   # Just Lower
                  | [A-Z]+   # Just Upper
                  | [0-9]+   # Just Digit");

            regex.Match("123Abc", 1).Value.ShouldEqual("23");
            regex.Match("$23ab!", 0).Success.ShouldBeFalse();
            regex.Match("$23ab!", 1).Value.ShouldEqual("23");
            regex.Match("$23ab!", 3).Value.ShouldEqual("ab");
        }

        [Fact]
        public void HasStringRepresentation()
        {
            new TokenRegex(@"[a-zA-Z0-9]*").ToString().ShouldEqual(@"[a-zA-Z0-9]*");
        }
    }
}