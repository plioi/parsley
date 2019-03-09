namespace Parsimonious.Tests
{
    using System.Collections.Generic;
    using Shouldly;
    using Xunit;

    public class TokenStreamTests
    {
        readonly TokenKind lower;
        readonly TokenKind upper;

        public TokenStreamTests()
        {
            lower = new Pattern("Lowercase", @"[a-z]+");
            upper = new Pattern("Uppercase", @"[A-Z]+");
        }

        IEnumerable<Token> Empty()
        {
            yield break;
        }

        IEnumerable<Token> OneToken()
        {
            yield return new Token(upper, new Position(1, 1), "ABC");
        }

        IEnumerable<Token> Tokens()
        {
            yield return new Token(upper, new Position(1, 1), "ABC");
            yield return new Token(lower, new Position(1, 4), "def");
            yield return new Token(upper, new Position(1, 7), "GHI");
            yield return new Token(TokenKind.EndOfInput, new Position(1, 10), "");
        }

        [Fact]
        public void ProvidesEndOfInputTokenWhenGivenEmptyEnumerator()
        {
            var tokens = new TokenStream(Empty());

            tokens.Current.ShouldBe(TokenKind.EndOfInput, "", 1, 1);
            tokens.Advance().ShouldBeSameAs(tokens);
        }

        [Fact]
        public void ProvidesCurrentToken()
        {
            var tokens = new TokenStream(Tokens());
            tokens.Current.ShouldBe(upper, "ABC", 1, 1);
        }

        [Fact]
        public void AdvancesToTheNextToken()
        {
            var tokens = new TokenStream(Tokens());
            tokens.Advance().Current.ShouldBe(lower, "def", 1, 4);
        }

        [Fact]
        public void ProvidesEndOfInputTokenAfterEnumeratorIsExhausted()
        {
            var tokens = new TokenStream(OneToken());
            var end = tokens.Advance();

            end.Current.ShouldBe(TokenKind.EndOfInput, "", 1, 4);
            end.Advance().ShouldBeSameAs(end);
        }

        [Fact]
        public void TryingToAdvanceBeyondEndOfInputResultsInNoMovement()
        {
            var tokens = new TokenStream(Empty());
            tokens.ShouldBeSameAs(tokens.Advance());
        }

        [Fact]
        public void DoesNotChangeStateAsUnderlyingEnumeratorIsTraversed()
        {
            var tokens = new TokenStream(Tokens());
            
            var first = tokens;

            first.Current.ShouldBe(upper, "ABC", 1, 1);

            var second = first.Advance();
            first.Current.ShouldBe(upper, "ABC", 1, 1);
            second.Current.ShouldBe(lower, "def", 1, 4);

            var third = second.Advance();
            first.Current.ShouldBe(upper, "ABC", 1, 1);
            second.Current.ShouldBe(lower, "def", 1, 4);
            third.Current.ShouldBe(upper, "GHI", 1, 7);

            var fourth = third.Advance();
            first.Current.ShouldBe(upper, "ABC", 1, 1);
            second.Current.ShouldBe(lower, "def", 1, 4);
            third.Current.ShouldBe(upper, "GHI", 1, 7);
            fourth.Current.ShouldBe(TokenKind.EndOfInput, "", 1, 10);

            fourth.Advance().ShouldBeSameAs(fourth);
        }

        [Fact]
        public void AllowsRepeatableTraversalWhileTraversingUnderlyingEnumeratorItemsAtMostOnce()
        {
            var tokens = new TokenStream(Tokens());

            tokens.Current.ShouldBe(upper, "ABC", 1, 1);
            tokens.Advance().Current.ShouldBe(lower, "def", 1, 4);
            tokens.Advance().Advance().Current.ShouldBe(upper, "GHI", 1, 7);
            tokens.Advance().Advance().Advance().Current.ShouldBe(TokenKind.EndOfInput, "", 1, 10);

            tokens.Advance().ShouldBeSameAs(tokens.Advance());
        }

        [Fact]
        public void ProvidesPositionOfCurrentToken()
        {
            var tokens = new TokenStream(Tokens());
            
            tokens.Position.Line.ShouldBe(1);
            tokens.Position.Column.ShouldBe(1);

            tokens.Advance().Position.Line.ShouldBe(1);
            tokens.Advance().Position.Column.ShouldBe(4);

            tokens.Advance().Advance().Position.Line.ShouldBe(1);
            tokens.Advance().Advance().Position.Column.ShouldBe(7);

            tokens.Advance().Advance().Advance().Position.Line.ShouldBe(1);
            tokens.Advance().Advance().Advance().Position.Column.ShouldBe(10);
        }
    }
}