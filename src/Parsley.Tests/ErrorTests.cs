namespace Parsley.Tests
{
    using System;
    using Shouldly;
    using Xunit;

    public class ErrorTests
    {
        readonly TokenStream x;
        readonly TokenStream endOfInput;

        public ErrorTests()
        {
            var lexer = new Lexer();
            x = new TokenStream(lexer.Tokenize("x"));
            endOfInput = new TokenStream(lexer.Tokenize(""));
        }

        [Fact]
        public void CanIndicateErrorsAtTheCurrentPosition()
        {
            new Error<object>(endOfInput, ErrorMessage.Unknown()).ErrorMessages.ToString().ShouldBe("Parse error.");
            new Error<object>(endOfInput, ErrorMessage.Expected("statement")).ErrorMessages.ToString().ShouldBe("statement expected");
        }

        [Fact]
        public void CanIndicateMultipleErrorsAtTheCurrentPosition()
        {
            var errors = ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"));

            new Error<object>(endOfInput, errors).ErrorMessages.ToString().ShouldBe("A or B expected");
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
            new Error<object>(x, ErrorMessage.Unknown()).UnparsedTokens.ShouldBe(x);
            new Error<object>(endOfInput, ErrorMessage.Unknown()).UnparsedTokens.ShouldBe(endOfInput);
        }

        [Fact]
        public void ReportsErrorState()
        {
            new Error<object>(x, ErrorMessage.Unknown()).Success.ShouldBeFalse();
        }
    }
}