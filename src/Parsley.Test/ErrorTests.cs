using System;
using Should;
using Xunit;

namespace Parsley
{
    public class ErrorTests
    {
        private readonly Lexer x;
        private readonly Lexer endOfInput;

        public ErrorTests()
        {
            x = new Lexer(new Text("x"));
            endOfInput = new Lexer(new Text(""));
        }

        [Fact]
        public void CanIndicateErrorsAtTheCurrentPosition()
        {
            new Error<object>(endOfInput, ErrorMessage.Unknown()).ErrorMessages.ToString().ShouldEqual("Parse error.");
            new Error<object>(endOfInput, ErrorMessage.Expected("statement")).ErrorMessages.ToString().ShouldEqual("statement expected");
        }

        [Fact]
        public void CanIndicateMultipleErrorsAtTheCurrentPosition()
        {
            var errors = ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"));

            new Error<object>(endOfInput, errors).ErrorMessages.ToString().ShouldEqual("A or B expected");
        }

        [Fact]
        public void ThrowsWhenAttemptingToGetParsedValue()
        {
            Func<object> inspectParsedValue = () => new Error<object>(x, ErrorMessage.Unknown()).Value;
            inspectParsedValue.ShouldThrow<MemberAccessException>("(1, 1): Parse error.");
        }

        [Fact]
        public void HasRemainingUnparsedTokens()
        {
            new Error<object>(x, ErrorMessage.Unknown()).UnparsedTokens.ShouldEqual(x);
            new Error<object>(endOfInput, ErrorMessage.Unknown()).UnparsedTokens.ShouldEqual(endOfInput);
        }

        [Fact]
        public void ReportsErrorState()
        {
            new Error<object>(x, ErrorMessage.Unknown()).Success.ShouldBeFalse();
        }
    }
}