namespace Parsley.Tests;

class ErrorTests
{
    readonly Text x;
    readonly Text endOfInput;

    public ErrorTests()
    {
        x = new Text("x");
        endOfInput = new Text("");
    }

    public void CanIndicateErrorsAtTheCurrentPosition()
    {
        new Error<object>(endOfInput.Position, endOfInput.EndOfInput, ErrorMessage.Unknown()).ErrorMessages.ToString().ShouldBe("Parse error.");
        new Error<object>(endOfInput.Position, endOfInput.EndOfInput, ErrorMessage.Expected("statement")).ErrorMessages.ToString().ShouldBe("statement expected");
    }

    public void CanIndicateMultipleErrorsAtTheCurrentPosition()
    {
        var errors = ErrorMessageList.Empty
            .With(ErrorMessage.Expected("A"))
            .With(ErrorMessage.Expected("B"));

        new Error<object>(endOfInput.Position, endOfInput.EndOfInput, errors).ErrorMessages.ToString().ShouldBe("A or B expected");
    }

    public void ThrowsWhenAttemptingToGetParsedValue()
    {
        var inspectParsedValue = () => new Error<object>(x.Position, x.EndOfInput, ErrorMessage.Unknown()).Value;
        inspectParsedValue
            .ShouldThrow<MemberAccessException>()
            .Message.ShouldBe("(1, 1): Parse error.");
    }

    public void HasRemainingUnparsedInput()
    {
        var xError = new Error<object>(x.Position, x.EndOfInput, ErrorMessage.Unknown());
        xError.Position.ShouldBe(x.Position);
        xError.EndOfInput.ShouldBe(x.EndOfInput);

        var endError = new Error<object>(endOfInput.Position, endOfInput.EndOfInput, ErrorMessage.Unknown());
        endError.Position.ShouldBe(endOfInput.Position);
        endError.EndOfInput.ShouldBe(endOfInput.EndOfInput);
    }

    public void ReportsErrorState()
    {
        new Error<object>(x.Position, x.EndOfInput, ErrorMessage.Unknown()).Success.ShouldBeFalse();
    }
}
