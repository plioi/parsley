namespace Parsley.Tests;

class ParsedTests
{
    public void CanIndicateSuccessfullyParsedValueAtTheCurrentPosition()
    {
        var parsed = new Parsed<string>("parsed", new(12, 34));
        parsed.Success.ShouldBe(true);
        parsed.Value.ShouldBe("parsed");
        parsed.ErrorMessages.ShouldBe(ErrorMessageList.Empty);
        parsed.Position.ShouldBe(new(12, 34));
    }

    public void CanIndicatePotentialErrorMessagesAtTheCurrentPosition()
    {
        var potentialErrors = ErrorMessageList.Empty
            .With(ErrorMessage.Expected("A"))
            .With(ErrorMessage.Expected("B"));

        var parsed = new Parsed<object>("parsed", new(12, 34), potentialErrors);
        parsed.Success.ShouldBe(true);
        parsed.Value.ShouldBe("parsed");
        parsed.ErrorMessages.ShouldBe(potentialErrors);
        parsed.Position.ShouldBe(new(12, 34));
    }
}
