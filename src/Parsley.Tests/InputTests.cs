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

        input.Current.ShouldBe(TokenKind.EndOfInput, "");
        input.Advance().ShouldBeSameAs(input);
    }

    public void ProvidesCurrentToken()
    {
        var input = new Input(Tokens());
        input.Current.ShouldBe(upper, "ABC");
    }

    public void AdvancesToTheNextToken()
    {
        var input = new Input(Tokens());
        input.Advance().Current.ShouldBe(lower, "def");
    }

    public void ProvidesEndOfInputTokenAfterEnumeratorIsExhausted()
    {
        var input = new Input(OneToken());
        var end = input.Advance();

        end.Current.ShouldBe(TokenKind.EndOfInput, "");
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

        first.Current.ShouldBe(upper, "ABC");

        var second = first.Advance();
        first.Current.ShouldBe(upper, "ABC");
        second.Current.ShouldBe(lower, "def");

        var third = second.Advance();
        first.Current.ShouldBe(upper, "ABC");
        second.Current.ShouldBe(lower, "def");
        third.Current.ShouldBe(upper, "GHI");

        var fourth = third.Advance();
        first.Current.ShouldBe(upper, "ABC");
        second.Current.ShouldBe(lower, "def");
        third.Current.ShouldBe(upper, "GHI");
        fourth.Current.ShouldBe(TokenKind.EndOfInput, "");

        fourth.Advance().ShouldBeSameAs(fourth);
    }

    public void AllowsRepeatableTraversal()
    {
        var input = new Input(Tokens());

        input.Current.ShouldBe(upper, "ABC");

        input.Advance().Current.ShouldBe(lower, "def");
        input.Advance().Current.ShouldBe(lower, "def");

        input.Advance().Advance().Current.ShouldBe(upper, "GHI");
        input.Advance().Advance().Current.ShouldBe(upper, "GHI");

        input.Advance().Advance().Advance().Current.ShouldBe(TokenKind.EndOfInput, "");
        input.Advance().Advance().Advance().Current.ShouldBe(TokenKind.EndOfInput, "");
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
