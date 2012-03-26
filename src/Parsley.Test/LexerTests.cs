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
        public void ProvidesEmptyEnumerableForEmptyText()
        {
            var tokens = new Lexer().Tokenize(new Text("")).ToArray();

            tokens.ShouldBeEmpty();
        }

        [Fact]
        public void UsesPrioritizedTokenMatchersToTokenize()
        {
            var tokens = new Lexer(lower, upper).Tokenize(new Text("ABCdefGHI")).ToArray();

            tokens.Length.ShouldEqual(3);
            tokens[0].ShouldBe(upper, "ABC", 1, 1);
            tokens[1].ShouldBe(lower, "def", 1, 4);
            tokens[2].ShouldBe(upper, "GHI", 1, 7);
        }

        [Fact]
        public void ProvidesTokenAtUnrecognizedInput()
        {
            var tokens = new Lexer(upper, lower).Tokenize(new Text("ABC!def")).ToArray();

            tokens.Length.ShouldEqual(2);
            tokens[0].ShouldBe(upper, "ABC", 1, 1);
            tokens[1].ShouldBe(TokenKind.Unknown, "!def", 1, 4);
        }

        [Fact]
        public void SkipsPastSkippableTokens()
        {
            var space = new Pattern("Space", @"\s", skippable: true);

            var tokens = new Lexer(lower, upper, space).Tokenize(new Text(" ")).ToArray();
            tokens.ShouldBeEmpty();

            tokens = new Lexer(lower, upper, space).Tokenize(new Text(" ABC  def   GHI    jkl  ")).ToArray();
            tokens[0].ShouldBe(upper, "ABC", 1, 2);
            tokens[1].ShouldBe(lower, "def", 1, 7);
            tokens[2].ShouldBe(upper, "GHI", 1, 13);
            tokens[3].ShouldBe(lower, "jkl", 1, 20);
        }
    }
}