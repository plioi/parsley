using System.Text.RegularExpressions;
using Should;

namespace Parsley
{
    public class TokenRegexTests
    {
        public void CanMatchRegexStartingFromGivenCharacterPosition()
        {
            var regex = new TokenRegex(@"[a-z]+");

            regex.Match("123abc0", 0).Success.ShouldBeFalse();

            regex.Match("123abc0", 3).Success.ShouldBeTrue();
            regex.Match("123abc0", 3).Value.ShouldEqual("abc");

            regex.Match("123abc0", 4).Success.ShouldBeTrue();
            regex.Match("123abc0", 4).Value.ShouldEqual("bc");
        }

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

        public void SupportsOptionalRegexOptions()
        {
            var regex = new TokenRegex(@"[a-z]+", RegexOptions.IgnoreCase);

            regex.Match("123aBc0", 0).Success.ShouldBeFalse();

            regex.Match("123aBc0", 3).Success.ShouldBeTrue();
            regex.Match("123aBc0", 3).Value.ShouldEqual("aBc");

            regex.Match("123aBc0", 4).Success.ShouldBeTrue();
            regex.Match("123aBc0", 4).Value.ShouldEqual("Bc");
        }

        public void HasStringRepresentation()
        {
            new TokenRegex(@"[a-zA-Z0-9]*").ToString().ShouldEqual(@"[a-zA-Z0-9]*");
        }
    }
}