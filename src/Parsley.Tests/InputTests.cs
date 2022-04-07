namespace Parsley.Tests;

class InputTests
{
    readonly TokenKind lower;
    readonly TokenKind upper;

    public InputTests()
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

    public void ProvidesEndOfInputTokenWhenGivenEmptyEnumerator()
    {
        var input = new Input(Empty());

        input.Current.ShouldBe(TokenKind.EndOfInput, "", 1, 1);
        input.Advance().ShouldBeSameAs(input);
    }

    public void ProvidesCurrentToken()
    {
        var input = new Input(Tokens());
        input.Current.ShouldBe(upper, "ABC", 1, 1);
    }

    public void AdvancesToTheNextToken()
    {
        var input = new Input(Tokens());
        input.Advance().Current.ShouldBe(lower, "def", 1, 4);
    }

    public void ProvidesEndOfInputTokenAfterEnumeratorIsExhausted()
    {
        var input = new Input(OneToken());
        var end = input.Advance();

        end.Current.ShouldBe(TokenKind.EndOfInput, "", 1, 4);
        end.Advance().ShouldBeSameAs(end);
    }

    public void TryingToAdvanceBeyondEndOfInputResultsInNoMovement()
    {
        var input = new Input(Empty());
        input.ShouldBeSameAs(input.Advance());
    }

    public void DoesNotChangeStateAsUnderlyingEnumeratorIsTraversed()
    {
        var input = new Input(Tokens());
            
        var first = input;

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

    public void AllowsRepeatableTraversal()
    {
        var input = new Input(Tokens());

        input.Current.ShouldBe(upper, "ABC", 1, 1);

        input.Advance().Current.ShouldBe(lower, "def", 1, 4);
        input.Advance().Current.ShouldBe(lower, "def", 1, 4);

        input.Advance().Advance().Current.ShouldBe(upper, "GHI", 1, 7);
        input.Advance().Advance().Current.ShouldBe(upper, "GHI", 1, 7);

        input.Advance().Advance().Advance().Current.ShouldBe(TokenKind.EndOfInput, "", 1, 10);
        input.Advance().Advance().Advance().Current.ShouldBe(TokenKind.EndOfInput, "", 1, 10);
    }

    public void ProvidesPositionOfCurrentToken()
    {
        var input = new Input(Tokens());
            
        input.Position.Line.ShouldBe(1);
        input.Position.Column.ShouldBe(1);

        input.Advance().Position.Line.ShouldBe(1);
        input.Advance().Position.Column.ShouldBe(4);

        input.Advance().Advance().Position.Line.ShouldBe(1);
        input.Advance().Advance().Position.Column.ShouldBe(7);

        input.Advance().Advance().Advance().Position.Line.ShouldBe(1);
        input.Advance().Advance().Advance().Position.Column.ShouldBe(10);
    }
}
