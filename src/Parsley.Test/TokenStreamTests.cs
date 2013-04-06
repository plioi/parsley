using System.Collections.Generic;
using Should;

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

        private IEnumerable<Token> Empty()
        {
            yield break;
        }

        private IEnumerable<Token> OneToken()
        {
            yield return new Token(upper, new Position(1, 1), "ABC");
        }

        private IEnumerable<Token> Tokens()
        {
            yield return new Token(upper, new Position(1, 1), "ABC");
            yield return new Token(lower, new Position(1, 4), "def");
            yield return new Token(upper, new Position(1, 7), "GHI");
            yield return new Token(TokenKind.EndOfInput, new Position(1, 10), "");
        }

        public void ProvidesEndOfInputTokenWhenGivenEmptyEnumerator()
        {
            var tokens = new TokenStream(Empty());

            tokens.Current.ShouldEqual(TokenKind.EndOfInput, "", 1, 1);
            tokens.Advance().ShouldBeSameAs(tokens);
        }

        public void ProvidesCurrentToken()
        {
            var tokens = new TokenStream(Tokens());
            tokens.Current.ShouldEqual(upper, "ABC", 1, 1);
        }

        public void AdvancesToTheNextToken()
        {
            var tokens = new TokenStream(Tokens());
            tokens.Advance().Current.ShouldEqual(lower, "def", 1, 4);
        }

        public void ProvidesEndOfInputTokenAfterEnumeratorIsExhausted()
        {
            var tokens = new TokenStream(OneToken());
            var end = tokens.Advance();

            end.Current.ShouldEqual(TokenKind.EndOfInput, "", 1, 4);
            end.Advance().ShouldBeSameAs(end);
        }

        public void TryingToAdvanceBeyondEndOfInputResultsInNoMovement()
        {
            var tokens = new TokenStream(Empty());
            tokens.ShouldBeSameAs(tokens.Advance());
        }

        public void DoesNotChangeStateAsUnderlyingEnumeratorIsTraversed()
        {
            var tokens = new TokenStream(Tokens());
            
            var first = tokens;

            first.Current.ShouldEqual(upper, "ABC", 1, 1);

            var second = first.Advance();
            first.Current.ShouldEqual(upper, "ABC", 1, 1);
            second.Current.ShouldEqual(lower, "def", 1, 4);

            var third = second.Advance();
            first.Current.ShouldEqual(upper, "ABC", 1, 1);
            second.Current.ShouldEqual(lower, "def", 1, 4);
            third.Current.ShouldEqual(upper, "GHI", 1, 7);

            var fourth = third.Advance();
            first.Current.ShouldEqual(upper, "ABC", 1, 1);
            second.Current.ShouldEqual(lower, "def", 1, 4);
            third.Current.ShouldEqual(upper, "GHI", 1, 7);
            fourth.Current.ShouldEqual(TokenKind.EndOfInput, "", 1, 10);

            fourth.Advance().ShouldBeSameAs(fourth);
        }

        public void AllowsRepeatableTraversalWhileTraversingUnderlyingEnumeratorItemsAtMostOnce()
        {
            var tokens = new TokenStream(Tokens());

            tokens.Current.ShouldEqual(upper, "ABC", 1, 1);
            tokens.Advance().Current.ShouldEqual(lower, "def", 1, 4);
            tokens.Advance().Advance().Current.ShouldEqual(upper, "GHI", 1, 7);
            tokens.Advance().Advance().Advance().Current.ShouldEqual(TokenKind.EndOfInput, "", 1, 10);

            tokens.Advance().ShouldBeSameAs(tokens.Advance());
        }

        public void ProvidesPositionOfCurrentToken()
        {
            var tokens = new TokenStream(Tokens());
            
            tokens.Position.Line.ShouldEqual(1);
            tokens.Position.Column.ShouldEqual(1);

            tokens.Advance().Position.Line.ShouldEqual(1);
            tokens.Advance().Position.Column.ShouldEqual(4);

            tokens.Advance().Advance().Position.Line.ShouldEqual(1);
            tokens.Advance().Advance().Position.Column.ShouldEqual(7);

            tokens.Advance().Advance().Advance().Position.Line.ShouldEqual(1);
            tokens.Advance().Advance().Advance().Position.Column.ShouldEqual(10);
        }
    }
}