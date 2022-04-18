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
}
