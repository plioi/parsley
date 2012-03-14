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
            lower = new TokenKind("Lowercase", @"[a-z]+");
            upper = new TokenKind("Uppercase", @"[A-Z]+");
        }

        [Fact]
        public void ProvidesCurrentToken()
        {
            var lexer = new Lexer(new Text("ABCdef"), upper);
            lexer.CurrentToken.ShouldBe(upper, "ABC", 1, 1);
        }

        [Fact]
        public void AdvancesToTheNextToken()
        {
            var lexer = new Lexer(new Text("ABCdef"), upper, lower);
            lexer.Advance().CurrentToken.ShouldBe(lower, "def", 1, 4);
        }

        [Fact]
        public void ProvidesTokenAtEndOfInput()
        {
            var lexer = new Lexer(new Text(""));
            lexer.CurrentToken.ShouldBe(Lexer.EndOfInput, "", 1, 1);
        }

        [Fact]
        public void TryingToAdvanceBeyondEndOfInputResultsInNoMovement()
        {
            var lexer = new Lexer(new Text(""));
            lexer.ShouldBeSameAs(lexer.Advance());
        }

        [Fact]
        public void UsesPrioritizedTokenMatchersToGetCurrentToken()
        {
            var lexer = new Lexer(new Text("ABCdefGHI"), lower, upper);
            lexer.CurrentToken.ShouldBe(upper, "ABC", 1, 1);
            lexer.Advance().CurrentToken.ShouldBe(lower, "def", 1, 4);
            lexer.Advance().Advance().CurrentToken.ShouldBe(upper, "GHI", 1, 7);
            lexer.Advance().Advance().Advance().CurrentToken.ShouldBe(Lexer.EndOfInput, "", 1, 10);
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
            var space = new TokenKind("Space", @"\s", skippable: true);

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