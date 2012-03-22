using System.Linq;
using Should;
using Xunit;

namespace Parsley
{
    public class TokenStreamTests
    {
        private readonly TokenKind lower;
        private readonly TokenKind upper;

        public TokenStreamTests()
        {
            lower = new Pattern("Lowercase", @"[a-z]+");
            upper = new Pattern("Uppercase", @"[A-Z]+");
        }

        [Fact]
        public void ProvidesCurrentToken()
        {
            var tokens = new TokenStream(new Text("ABCdef"), upper);
            tokens.Current.ShouldBe(upper, "ABC", 1, 1);
        }

        [Fact]
        public void AdvancesToTheNextToken()
        {
            var tokens = new TokenStream(new Text("ABCdef"), upper, lower);
            tokens.Advance().Current.ShouldBe(lower, "def", 1, 4);
        }

        [Fact]
        public void ProvidesTokenAtEndOfInput()
        {
            var tokens = new TokenStream(new Text(""));
            tokens.Current.ShouldBe(TokenStream.EndOfInput, "", 1, 1);
        }

        [Fact]
        public void TryingToAdvanceBeyondEndOfInputResultsInNoMovement()
        {
            var tokens = new TokenStream(new Text(""));
            tokens.ShouldBeSameAs(tokens.Advance());
        }

        [Fact]
        public void UsesPrioritizedTokenMatchersToGetCurrentToken()
        {
            var tokens = new TokenStream(new Text("ABCdefGHI"), lower, upper);
            tokens.Current.ShouldBe(upper, "ABC", 1, 1);
            tokens.Advance().Current.ShouldBe(lower, "def", 1, 4);
            tokens.Advance().Advance().Current.ShouldBe(upper, "GHI", 1, 7);
            tokens.Advance().Advance().Advance().Current.ShouldBe(TokenStream.EndOfInput, "", 1, 10);
        }

        [Fact]
        public void CanBeEnumerated()
        {
            var tokens = new TokenStream(new Text("ABCdefGHIjkl"), lower, upper).ToArray();
            tokens.Length.ShouldEqual(5);
            tokens[0].ShouldBe(upper, "ABC", 1, 1);
            tokens[1].ShouldBe(lower, "def", 1, 4);
            tokens[2].ShouldBe(upper, "GHI", 1, 7);
            tokens[3].ShouldBe(lower, "jkl", 1, 10);
            tokens[4].ShouldBe(TokenStream.EndOfInput, "", 1, 13);
        }

        [Fact]
        public void ProvidesTokenAtUnrecognizedInput()
        {
            var tokens = new TokenStream(new Text("ABC!def"), upper, lower).ToArray();
            tokens.Length.ShouldEqual(3);
            tokens[0].ShouldBe(upper, "ABC", 1, 1);
            tokens[1].ShouldBe(TokenStream.Unknown, "!def", 1, 4);
            tokens[2].ShouldBe(TokenStream.EndOfInput, "", 1, 8);
        }

        [Fact]
        public void SkipsPastSkippableTokens()
        {
            var space = new Pattern("Space", @"\s", skippable: true);

            var tokens = new TokenStream(new Text(" "), lower, upper, space).ToArray();
            tokens.Length.ShouldEqual(1);
            tokens[0].ShouldBe(TokenStream.EndOfInput, "", 1, 2);

            tokens = new TokenStream(new Text(" ABC  def   GHI    jkl"), lower, upper, space).ToArray();
            tokens.Length.ShouldEqual(5);
            tokens[0].ShouldBe(upper, "ABC", 1, 2);
            tokens[1].ShouldBe(lower, "def", 1, 7);
            tokens[2].ShouldBe(upper, "GHI", 1, 13);
            tokens[3].ShouldBe(lower, "jkl", 1, 20);
            tokens[4].ShouldBe(TokenStream.EndOfInput, "", 1, 23);
        }
    }
}