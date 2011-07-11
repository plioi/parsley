using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class PatternTests
    {
        [Test]
        public void CanMatchRegexStartingFromGivenCharacterPosition()
        {
            var pattern = new Pattern(@"[a-z]+");
            
            pattern.Match("123abc0", 0).Success.ShouldBeFalse();
            
            pattern.Match("123abc0", 3).Success.ShouldBeTrue();
            pattern.Match("123abc0", 3).Value.ShouldEqual("abc");
            
            pattern.Match("123abc0", 4).Success.ShouldBeTrue();
            pattern.Match("123abc0", 4).Value.ShouldEqual("bc");
        }

        [Test]
        public void CanMatchMultilineAndCommentedRegexes()
        {
            var pattern = new Pattern(
                @"  [a-z]+   # Just Lower
                  | [A-Z]+   # Just Upper
                  | [0-9]+   # Just Digit");

            pattern.Match("123Abc", 1).Value.ShouldEqual("23");
            pattern.Match("$23ab!", 0).Success.ShouldBeFalse();
            pattern.Match("$23ab!", 1).Value.ShouldEqual("23");
            pattern.Match("$23ab!", 3).Value.ShouldEqual("ab");
        }

        [Test]
        public void HasStringRepresentation()
        {
            new Pattern(@"[a-zA-Z0-9]*").ToString().ShouldEqual(@"[a-zA-Z0-9]*");
        }
    }
}