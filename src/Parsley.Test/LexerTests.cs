using System.Collections.Generic;
using Should;
using Xunit;

namespace Parsley
{
    public class LexerTests
    {
        private readonly TokenKind lower;
        private readonly TokenKind upper;
        private readonly TokenKind space;

        public LexerTests()
        {
            lower = new Pattern("Lowercase", @"[a-z]+");
            upper = new Pattern("Uppercase", @"[A-Z]+");
            space = new Pattern("Space", @"\s", skippable: true);
        }

        private IEnumerable<Token> Tokenize(string input)
        {
            return new Lexer(lower, upper, space).Tokenize(new Text(input));
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
                .ShouldList(t => t.ShouldEqual(upper, "ABC", 1, 1),
                            t => t.ShouldEqual(lower, "def", 1, 4),
                            t => t.ShouldEqual(upper, "GHI", 1, 7));
        }

        [Fact]
        public void ProvidesTokenAtUnrecognizedInput()
        {
            Tokenize("ABC!def")
                .ShouldList(t => t.ShouldEqual(upper, "ABC", 1, 1),
                            t => t.ShouldEqual(TokenKind.Unknown, "!def", 1, 4));
        }

        [Fact]
        public void SkipsPastSkippableTokens()
        {
            Tokenize(" ").ShouldBeEmpty();

            Tokenize(" ABC  def   GHI    jkl  ")
                .ShouldList(t => t.ShouldEqual(upper, "ABC", 1, 2),
                            t => t.ShouldEqual(lower, "def", 1, 7),
                            t => t.ShouldEqual(upper, "GHI", 1, 13),
                            t => t.ShouldEqual(lower, "jkl", 1, 20));
        }
    }
}