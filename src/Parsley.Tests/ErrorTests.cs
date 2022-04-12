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
        new Error<object>(endOfInput, ErrorMessage.Unknown()).ErrorMessages.ToString().ShouldBe("Parse error.");
        new Error<object>(endOfInput, ErrorMessage.Expected("statement")).ErrorMessages.ToString().ShouldBe("statement expected");
    }

    public void CanIndicateMultipleErrorsAtTheCurrentPosition()
    {
        var errors = ErrorMessageList.Empty
            .With(ErrorMessage.Expected("A"))
            .With(ErrorMessage.Expected("B"));

        new Error<object>(endOfInput, errors).ErrorMessages.ToString().ShouldBe("A or B expected");
    }

    public void ThrowsWhenAttemptingToGetParsedValue()
    {
        var inspectParsedValue = () => new Error<object>(x, ErrorMessage.Unknown()).Value;
        inspectParsedValue
            .ShouldThrow<MemberAccessException>()
            .Message.ShouldBe("(1, 1): Parse error.");
    }

    public void HasRemainingUnparsedInput()
    {
        new Error<object>(x, ErrorMessage.Unknown()).UnparsedInput.ShouldBe(x);
        new Error<object>(endOfInput, ErrorMessage.Unknown()).UnparsedInput.ShouldBe(endOfInput);
    }

    public void ReportsErrorState()
    {
        new Error<object>(x, ErrorMessage.Unknown()).Success.ShouldBeFalse();
    }
}
