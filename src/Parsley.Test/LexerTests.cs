using System.Linq;
using Should;
using Xunit;

namespace Parsley
{
    public class LexerTests
    {
        private readonly TokenKind lower;
        private readonly TokenKind upper;

        public LexerTests()
        {
            lower = new Pattern("Lowercase", @"[a-z]+");
            upper = new Pattern("Uppercase", @"[A-Z]+");
        }

        [Fact]
        public void ProvidesCurrentToken()
        {
            var tokens = new Lexer(new Text("ABCdef"), upper);
            tokens.Current.ShouldBe(upper, "ABC", 1, 1);
        }

        [Fact]
        public void AdvancesToTheNextToken()
        {
            var tokens = new Lexer(new Text("ABCdef"), upper, lower);
            tokens.Advance().Current.ShouldBe(lower, "def", 1, 4);
        }

        [Fact]
        public void ProvidesTokenAtEndOfInput()
        {
            var tokens = new Lexer(new Text(""));
            tokens.Current.ShouldBe(Lexer.EndOfInput, "", 1, 1);
        }

        [Fact]
        public void TryingToAdvanceBeyondEndOfInputResultsInNoMovement()
        {
            var tokens = new Lexer(new Text(""));
            tokens.ShouldBeSameAs(tokens.Advance());
        }

        [Fact]
        public void UsesPrioritizedTokenMatchersToGetCurrentToken()
        {
            var tokens = new Lexer(new Text("ABCdefGHI"), lower, upper);
            tokens.Current.ShouldBe(upper, "ABC", 1, 1);
            tokens.Advance().Current.ShouldBe(lower, "def", 1, 4);
            tokens.Advance().Advance().Current.ShouldBe(upper, "GHI", 1, 7);
            tokens.Advance().Advance().Advance().Current.ShouldBe(Lexer.EndOfInput, "", 1, 10);
        }

        [Fact]
        public void CanBeEnumerated()
        {
            var tokens = new Lexer(new Text("ABCdefGHIjkl"), lower, upper).ToArray();
            tokens.Length.ShouldEqual(5);
            tokens[0].ShouldBe(upper, "ABC", 1, 1);
            tokens[1].ShouldBe(lower, "def", 1, 4);
            tokens[2].ShouldBe(upper, "GHI", 1, 7);
            tokens[3].ShouldBe(lower, "jkl", 1, 10);
            tokens[4].ShouldBe(Lexer.EndOfInput, "", 1, 13);
        }

        [Fact]
        public void ProvidesTokenAtUnrecognizedInput()
        {
            var tokens = new Lexer(new Text("ABC!def"), upper, lower).ToArray();
            tokens.Length.ShouldEqual(3);
            tokens[0].ShouldBe(upper, "ABC", 1, 1);
            tokens[1].ShouldBe(Lexer.Unknown, "!def", 1, 4);
            tokens[2].ShouldBe(Lexer.EndOfInput, "", 1, 8);
        }

        [Fact]
        public void SkipsPastSkippableTokens()
        {
            var space = new Pattern("Space", @"\s", skippable: true);

            var tokens = new Lexer(new Text(" "), lower, upper, space).ToArray();
            tokens.Length.ShouldEqual(1);
            tokens[0].ShouldBe(Lexer.EndOfInput, "", 1, 2);

            tokens = new Lexer(new Text(" ABC  def   GHI    jkl"), lower, upper, space).ToArray();
            tokens.Length.ShouldEqual(5);
            tokens[0].ShouldBe(upper, "ABC", 1, 2);
            tokens[1].ShouldBe(lower, "def", 1, 7);
            tokens[2].ShouldBe(upper, "GHI", 1, 13);
            tokens[3].ShouldBe(lower, "jkl", 1, 20);
            tokens[4].ShouldBe(Lexer.EndOfInput, "", 1, 23);
        }
    }
}