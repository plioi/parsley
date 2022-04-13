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
        new Error<object>(endOfInput.Position, ErrorMessage.Unknown()).ErrorMessages.ToString().ShouldBe("Parse error.");
        new Error<object>(endOfInput.Position, ErrorMessage.Expected("statement")).ErrorMessages.ToString().ShouldBe("statement expected");
    }

    public void CanIndicateMultipleErrorsAtTheCurrentPosition()
    {
        var errors = ErrorMessageList.Empty
            .With(ErrorMessage.Expected("A"))
            .With(ErrorMessage.Expected("B"));

        new Error<object>(endOfInput.Position, errors).ErrorMessages.ToString().ShouldBe("A or B expected");
    }

    public void ThrowsWhenAttemptingToGetParsedValue()
    {
        var inspectParsedValue = () => new Error<object>(x.Position, ErrorMessage.Unknown()).Value;
        inspectParsedValue
            .ShouldThrow<MemberAccessException>()
            .Message.ShouldBe("(1, 1): Parse error.");
    }

    public void HasRemainingUnparsedInput()
    {
        var xError = new Error<object>(x.Position, ErrorMessage.Unknown());
        xError.Position.ShouldBe(x.Position);

        var endError = new Error<object>(endOfInput.Position, ErrorMessage.Unknown());
        endError.Position.ShouldBe(endOfInput.Position);
    }

    public void ReportsErrorState()
    {
        new Error<object>(x.Position, ErrorMessage.Unknown()).Success.ShouldBeFalse();
    }
}
