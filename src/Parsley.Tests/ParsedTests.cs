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
        new Parsed<string>("parsed", unparsed, unparsed.Position, unparsed.EndOfInput).Value.ShouldBe("parsed");
    }

    public void HasNoErrorMessageByDefault()
    {
        new Parsed<string>("x", unparsed, unparsed.Position, unparsed.EndOfInput).ErrorMessages.ShouldBe(ErrorMessageList.Empty);
    }

    public void CanIndicatePotentialErrors()
    {
        var potentialErrors = ErrorMessageList.Empty
            .With(ErrorMessage.Expected("A"))
            .With(ErrorMessage.Expected("B"));

        new Parsed<object>("x", unparsed, unparsed.Position, unparsed.EndOfInput, potentialErrors).ErrorMessages.ShouldBe(potentialErrors);
    }

    public void HasRemainingUnparsedInput()
    {
        new Parsed<string>("parsed", unparsed, unparsed.Position, unparsed.EndOfInput).UnparsedInput.ShouldBe(unparsed);
    }

    public void ReportsNonerrorState()
    {
        new Parsed<string>("parsed", unparsed, unparsed.Position, unparsed.EndOfInput).Success.ShouldBeTrue();
    }
}
