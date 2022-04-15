namespace Parsley.Tests;

class ErrorTests
{
    public void CanIndicateErrorsAtTheCurrentPosition()
    {
        var error = new Error<object>(new(12, 34), ErrorMessage.Unknown());
        error.Success.ShouldBe(false);
        error.ErrorMessages.ToString().ShouldBe("Parse error.");
        error.Position.ShouldBe(new(12, 34));

        error = new Error<object>(new(23, 45), ErrorMessage.Expected("statement"));
        error.Success.ShouldBe(false);
        error.ErrorMessages.ToString().ShouldBe("statement expected");
        error.Position.ShouldBe(new(23, 45));
    }

    public void CanIndicateMultipleErrorsAtTheCurrentPosition()
    {
       var errors = ErrorMessageList.Empty
            .With(ErrorMessage.Expected("A"))
            .With(ErrorMessage.Expected("B"));

       var error = new Error<object>(new(12, 34), errors);
       error.Success.ShouldBe(false);
       error.ErrorMessages.ToString().ShouldBe("A or B expected");
       error.Position.ShouldBe(new(12, 34));
    }

    public void ThrowsWhenAttemptingToGetParsedValue()
    {
        var inspectParsedValue = () => new Error<object>(new(12, 34), ErrorMessage.Unknown()).Value;
        inspectParsedValue
            .ShouldThrow<MemberAccessException>()
            .Message.ShouldBe("Parse error.");
    }
}
