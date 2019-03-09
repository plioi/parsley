namespace Parsimonious.Tests
{
    using System.Collections.Generic;
    using Shouldly;
    using Xunit;

    public class LexerTests
    {
        readonly TokenKind lower;
        readonly TokenKind upper;
        readonly TokenKind space;

        public LexerTests()
        {
            lower = new Pattern("Lowercase", @"[a-z]+");
            upper = new Pattern("Uppercase", @"[A-Z]+");
            space = new Pattern("Space", @"\s", skippable: true);
        }

        IEnumerable<Token> Tokenize(string input)
        {
            return new Lexer(lower, upper, space).Tokenize(input);
        }

        [Fact]
        public void ProvidesEmptyEnumerableForEmptyText()
        {
            Tokenize("").ShouldBeEmpty();
        }

        [Fact]
        public void UsesPrioritizedTokenMatchersToTokenize()
        {
            Tokenize("ABCdefGHI")
                .ShouldList(t => t.ShouldBe(upper, "ABC", 1, 1),
                            t => t.ShouldBe(lower, "def", 1, 4),
                            t => t.ShouldBe(upper, "GHI", 1, 7));
        }

        [Fact]
        public void ProvidesTokenAtUnrecognizedInput()
        {
            Tokenize("ABC!def")
                .ShouldList(t => t.ShouldBe(upper, "ABC", 1, 1),
                            t => t.ShouldBe(TokenKind.Unknown, "!def", 1, 4));
        }

        [Fact]
        public void SkipsPastSkippableTokens()
        {
            Tokenize(" ").ShouldBeEmpty();

            Tokenize(" ABC  def   GHI    jkl  ")
                .ShouldList(t => t.ShouldBe(upper, "ABC", 1, 2),
                            t => t.ShouldBe(lower, "def", 1, 7),
                            t => t.ShouldBe(upper, "GHI", 1, 13),
                            t => t.ShouldBe(lower, "jkl", 1, 20));
        }
    }
}