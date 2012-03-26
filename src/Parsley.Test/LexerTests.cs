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
        public void ProvidesEmptyEnumeratorForEmptyText()
        {
            var tokens = new Lexer().Tokenize(new Text(""));
            
            tokens.MoveNext().ShouldBeFalse();
        }

        [Fact]
        public void UsesPrioritizedTokenMatchersToTokenize()
        {
            var tokens = new Lexer(lower, upper).Tokenize(new Text("ABCdefGHI"));

            tokens.MoveNext().ShouldBeTrue();
            tokens.Current.ShouldBe(upper, "ABC", 1, 1);

            tokens.MoveNext().ShouldBeTrue();
            tokens.Current.ShouldBe(lower, "def", 1, 4);
            
            tokens.MoveNext().ShouldBeTrue();
            tokens.Current.ShouldBe(upper, "GHI", 1, 7);
            
            tokens.MoveNext().ShouldBeFalse();
        }

        [Fact]
        public void ProvidesTokenAtUnrecognizedInput()
        {
            var tokens = new Lexer(upper, lower).Tokenize(new Text("ABC!def"));

            tokens.MoveNext().ShouldBeTrue();
            tokens.Current.ShouldBe(upper, "ABC", 1, 1);

            tokens.MoveNext().ShouldBeTrue();
            tokens.Current.ShouldBe(TokenKind.Unknown, "!def", 1, 4);

            tokens.MoveNext().ShouldBeFalse();
        }

        [Fact]
        public void SkipsPastSkippableTokens()
        {
            var space = new Pattern("Space", @"\s", skippable: true);

            var tokens = new Lexer(lower, upper, space).Tokenize(new Text(" "));
            tokens.MoveNext().ShouldBeFalse();


            tokens = new Lexer(lower, upper, space).Tokenize(new Text(" ABC  def   GHI    jkl  "));

            tokens.MoveNext().ShouldBeTrue();
            tokens.Current.ShouldBe(upper, "ABC", 1, 2);

            tokens.MoveNext().ShouldBeTrue();
            tokens.Current.ShouldBe(lower, "def", 1, 7);

            tokens.MoveNext().ShouldBeTrue();
            tokens.Current.ShouldBe(upper, "GHI", 1, 13);

            tokens.MoveNext().ShouldBeTrue();
            tokens.Current.ShouldBe(lower, "jkl", 1, 20);

            tokens.MoveNext().ShouldBeFalse();
        }
    }
}