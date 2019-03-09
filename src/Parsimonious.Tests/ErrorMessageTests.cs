namespace Parsimonious.Tests
{
    using Shouldly;
    using Xunit;
    using ErrorMessage = Parsimonious.ErrorMessage;

    public class ErrorMessageTests
    {
        [Fact]
        public void CanIndicateGenericErrors()
        {
            var error = ErrorMessage.Unknown();
            error.ToString().ShouldBe("Parse error.");
        }

        [Fact]
        public void CanIndicateSpecificExpectation()
        {
            var error = (ExpectedErrorMessage)ErrorMessage.Expected("statement");
            error.Expectation.ShouldBe("statement");
            error.ToString().ShouldBe("statement expected");
        }

        [Fact]
        public void CanIndicateErrorsWhichCausedBacktracking()
        {
            var position = new Position(3, 4);
            ErrorMessageList errors = ErrorMessageList.Empty
                .With(ErrorMessage.Expected("a"))
                .With(ErrorMessage.Expected("b"));

            var error = (BacktrackErrorMessage) ErrorMessage.Backtrack(position, errors);
            error.Position.ShouldBe(position);
            error.Errors.ShouldBe(errors);
            error.ToString().ShouldBe("(3, 4): a or b expected");
        }
    }
}