namespace Parsley.Tests;

class ParsedTests
{
    readonly Input unparsed;

    public ParsedTests()
    {
        unparsed = new Input(new CharLexer().Tokenize("0"));
    }

    public void HasAParsedValue()
    {
        new Parsed<string>("parsed", unparsed).Value.ShouldBe("parsed");
    }

    public void HasNoErrorMessageByDefault()
    {
        new Parsed<string>("x", unparsed).ErrorMessages.ShouldBe(ErrorMessageList.Empty);
    }

    public void CanIndicatePotentialErrors()
    {
        var potentialErrors = ErrorMessageList.Empty
            .With(ErrorMessage.Expected("A"))
            .With(ErrorMessage.Expected("B"));

        new Parsed<object>("x", unparsed, potentialErrors).ErrorMessages.ShouldBe(potentialErrors);
    }

    public void HasRemainingUnparsedInput()
    {
        new Parsed<string>("parsed", unparsed).UnparsedInput.ShouldBe(unparsed);
    }

    public void ReportsNonerrorState()
    {
        new Parsed<string>("parsed", unparsed).Success.ShouldBeTrue();
    }
}
