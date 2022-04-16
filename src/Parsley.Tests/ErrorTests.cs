namespace Parsley.Tests;

class ErrorTests
{
    public void CanIndicateErrors()
    {
        var error = new Error<object>(ErrorMessage.Unknown);
        error.Success.ShouldBe(false);
        error.ErrorMessages.ToString().ShouldBe("Parse error.");

        error = new Error<object>(ErrorMessage.Expected("statement"));
        error.Success.ShouldBe(false);
        error.ErrorMessages.ToString().ShouldBe("statement expected");
    }

    public void CanIndicateMultipleErrors()
    {
       var errors = ErrorMessageList.Empty
            .With(ErrorMessage.Expected("A"))
            .With(ErrorMessage.Expected("B"));

       var error = new Error<object>(errors);
       error.Success.ShouldBe(false);
       error.ErrorMessages.ToString().ShouldBe("A or B expected");
    }

    public void ThrowsWhenAttemptingToGetParsedValue()
    {
        var inspectParsedValue = () => new Error<object>(ErrorMessage.Unknown).Value;
        inspectParsedValue
            .ShouldThrow<MemberAccessException>()
            .Message.ShouldBe("Parse error.");
    }
}
