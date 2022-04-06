namespace Parsley.Tests
{
    using Shouldly;
    using ErrorMessage = Parsley.ErrorMessage;

    public class ErrorMessageTests
    {
        public void CanIndicateGenericErrors()
        {
            var error = ErrorMessage.Unknown();
            error.ToString().ShouldBe("Parse error.");
        }

        public void CanIndicateSpecificExpectation()
        {
            var error = (ExpectedErrorMessage)ErrorMessage.Expected("statement");
            error.Expectation.ShouldBe("statement");
            error.ToString().ShouldBe("statement expected");
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
            error.ToString().ShouldBe("(3, 4): a or b expected");
        }
    }
}