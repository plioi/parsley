namespace Parsley.Tests;

class ErrorMessageTests
{
    public void CanIndicateGenericErrors()
    {
        ErrorMessage.Unknown.ShouldBeSameAs(ErrorMessage.Unknown);
    }

    public void CanIndicateSpecificExpectation()
    {
        var error = (ExpectedErrorMessage)ErrorMessage.Expected("statement");
        error.Expectation.ShouldBe("statement");
    }

    public void CanIndicateErrorsWhichCausedBacktracking()
    {
        var position = new Position(3, 4);
        var errors = ErrorMessageList.Empty
            .With(ErrorMessage.Expected("a"))
            .With(ErrorMessage.Expected("b"));

        var error = (BacktrackErrorMessage) ErrorMessage.Backtrack(position, errors);
        error.Position.ShouldBe(position);
        error.Errors.ShouldBe(errors);
    }
}
