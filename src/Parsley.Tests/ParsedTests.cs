namespace Parsley.Tests;

class ParsedTests
{
    readonly Text unparsed;

    public ParsedTests()
    {
        unparsed = new Text("0");
    }

    public void HasAParsedValue()
    {
        new Parsed<string>("parsed", unparsed.Position).Value.ShouldBe("parsed");
    }

    public void HasNoErrorMessageByDefault()
    {
        new Parsed<string>("x", unparsed.Position).ErrorMessages.ShouldBe(ErrorMessageList.Empty);
    }

    public void CanIndicatePotentialErrors()
    {
        var potentialErrors = ErrorMessageList.Empty
            .With(ErrorMessage.Expected("A"))
            .With(ErrorMessage.Expected("B"));

        new Parsed<object>("x", unparsed.Position, potentialErrors).ErrorMessages.ShouldBe(potentialErrors);
    }

    public void HasRemainingUnparsedInput()
    {
        var parsed = new Parsed<string>("parsed", unparsed.Position);
        parsed.Position.ShouldBe(unparsed.Position);
    }

    public void ReportsNonerrorState()
    {
        new Parsed<string>("parsed", unparsed.Position).Success.ShouldBeTrue();
    }
}
