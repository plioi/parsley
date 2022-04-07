namespace Parsley.Tests;

class ErrorTests
{
    readonly TokenStream x;
    readonly TokenStream endOfInput;

    public ErrorTests()
    {
        var lexer = new Lexer();
        x = new TokenStream(lexer.Tokenize("x"));
        endOfInput = new TokenStream(lexer.Tokenize(""));
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
        Func<object> inspectParsedValue = () => new Error<object>(x, ErrorMessage.Unknown()).Value;
        inspectParsedValue.ShouldThrow<MemberAccessException>("(1, 1): Parse error.");
    }

    public void HasRemainingUnparsedTokens()
    {
        new Error<object>(x, ErrorMessage.Unknown()).UnparsedTokens.ShouldBe(x);
        new Error<object>(endOfInput, ErrorMessage.Unknown()).UnparsedTokens.ShouldBe(endOfInput);
    }

    public void ReportsErrorState()
    {
        new Error<object>(x, ErrorMessage.Unknown()).Success.ShouldBeFalse();
    }
}
