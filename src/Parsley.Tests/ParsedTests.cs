namespace Parsley.Tests;

class ParsedTests
{
    public void HasAParsedValue()
    {
        new Parsed<string>("parsed", new(1, 1)).Value.ShouldBe("parsed");
    }

    public void HasNoErrorMessageByDefault()
    {
        new Parsed<string>("x", new(1, 1)).ErrorMessages.ShouldBe(ErrorMessageList.Empty);
    }

    public void CanIndicatePotentialErrors()
    {
        var potentialErrors = ErrorMessageList.Empty
            .With(ErrorMessage.Expected("A"))
            .With(ErrorMessage.Expected("B"));

        new Parsed<object>("x", new(1, 1), potentialErrors).ErrorMessages.ShouldBe(potentialErrors);
    }

    public void HasRemainingUnparsedInput()
    {
        var parsed = new Parsed<string>("parsed", new(12, 34));
        parsed.Position.ShouldBe(new (12, 34));
    }

    public void ReportsNonerrorState()
    {
        new Parsed<string>("parsed", new(1, 1)).Success.ShouldBeTrue();
    }
}
