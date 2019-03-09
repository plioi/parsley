using Shouldly;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Parsimonious.Tests
{
    public class LinedLexerTests
    {
        readonly TokenKind _lower;
        readonly TokenKind _upper;

        public LinedLexerTests()
        {
            _lower = new Pattern("Lowercase", @"[a-z]+");
            _upper = new Pattern("Uppercase", @"[A-Z]+");
            TokenKind space = new Pattern("Space", @"\s", true);

            _lexer = new LinedLexer(_lower, _upper, space);
        }

        private readonly LinedLexer _lexer;

        IEnumerable<Token> Tokenize(string input)
        {
            return _lexer.Tokenize(new StringReader(input));
        }

        [Fact]
        public void ProvidesEmptyEnumerableForEmptyText()
        {
            Tokenize("").ShouldBeEmpty();
        }

        [Fact]
        public void UsesPrioritizedTokenMatchersToTokenize()
        {
            Tokenize("ABCdef\n" +
                     "GHI")
                .ShouldList(t => t.ShouldBe(_upper, "ABC", 1, 1),
                            t => t.ShouldBe(_lower, "def", 1, 4),
                            t => t.ShouldBe(_upper, "GHI", 2, 1));
        }

        [Fact]
        public void ProvidesTokenAtUnrecognizedInput()
        {
            Tokenize("ABC!def")
                .ShouldList(t => t.ShouldBe(_upper, "ABC", 1, 1),
                            t => t.ShouldBe(TokenKind.Unknown, "!def", 1, 4));
        }

        [Fact]
        public void ProvidesTokenAtUnrecognizedMultilineInput()
        {
            Tokenize(@"ABC
                      !def
                      vfs")
                .ShouldList(
                    t => t.ShouldBe(_upper, "ABC", 1, 1),
                    t => t.ShouldBe(TokenKind.Unknown, "!def", 2, 23));
        }

        [Fact]
        public void DoesNotReadPastUnrecognizedTokenLine()
        {
            var oneMoreLine = "one more line";
            var input = "ABC\n!def\n" + oneMoreLine;

            using (var reader = new StringReader(input))
            {
                var tokens = _lexer.Tokenize(reader);

                tokens.ShouldList(
                    t => t.ShouldBe(_upper, "ABC", 1, 1),
                    t => t.ShouldBe(TokenKind.Unknown, "!def", 2, 1));

                reader.ReadLine().ShouldBe(oneMoreLine);
            }
        }

        [Fact]
        public void SkipsPastSkippableTokens()
        {
            Tokenize(" ").ShouldBeEmpty();

            Tokenize(" ABC  def   GHI    jkl  ")
                .ShouldList(t => t.ShouldBe(_upper, "ABC", 1, 2),
                            t => t.ShouldBe(_lower, "def", 1, 7),
                            t => t.ShouldBe(_upper, "GHI", 1, 13),
                            t => t.ShouldBe(_lower, "jkl", 1, 20));
        }

        [Fact]
        public void TokenizesMultilineText()
        {
            Tokenize(" ").ShouldBeEmpty();

            Tokenize(@" ABC
                        def
                         GHI
                          jkl  ")
                .ShouldList(
                    t => t.ShouldBe(_upper, "ABC", 1, 2),
                    t => t.ShouldBe(_lower, "def", 2, 25),
                    t => t.ShouldBe(_upper, "GHI", 3, 26),
                    t => t.ShouldBe(_lower, "jkl", 4, 27));
        }
    }
}