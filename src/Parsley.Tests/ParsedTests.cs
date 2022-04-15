namespace Parsley.Tests;

class ParsedTests
{
    public void CanIndicateSuccessfullyParsedValue()
    {
        var parsed = new Parsed<string>("parsed");
        parsed.Success.ShouldBe(true);
        parsed.Value.ShouldBe("parsed");
        parsed.ErrorMessages.ShouldBe(ErrorMessageList.Empty);
    }

    public void CanIndicatePotentialErrorMessages()
    {
        var potentialErrors = ErrorMessageList.Empty
            .With(ErrorMessage.Expected("A"))
            .With(ErrorMessage.Expected("B"));

        var parsed = new Parsed<object>("parsed", potentialErrors);
        parsed.Success.ShouldBe(true);
        parsed.Value.ShouldBe("parsed");
        parsed.ErrorMessages.ShouldBe(potentialErrors);
    }
}
