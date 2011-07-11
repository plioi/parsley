using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class ErrorMessageTests
    {
        [Test]
        public void CanIndicateGenericErrors()
        {
            var error = ErrorMessage.Unknown();
            error.ToString().ShouldEqual("Parse error.");
        }

        [Test]
        public void CanIndicateSpecificExpectation()
        {
            var error = (ExpectedErrorMessage)ErrorMessage.Expected("statement");
            error.Expectation.ShouldEqual("statement");
            error.ToString().ShouldEqual("statement expected");
        }

        [Test]
        public void CanIndicateErrorsWhichCausedBacktracking()
        {
            var position = new Position(3, 4);
            ErrorMessageList errors = ErrorMessageList.Empty
                .With(ErrorMessage.Expected("a"))
                .With(ErrorMessage.Expected("b"));

            var error = (BacktrackErrorMessage) ErrorMessage.Backtrack(position, errors);
            error.Position.ShouldEqual(position);
            error.Errors.ShouldEqual(errors);
            error.ToString().ShouldEqual("(3, 4): a or b expected");
        }
    }
}