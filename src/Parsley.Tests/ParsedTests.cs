namespace Parsley.Tests;

class ParsedTests
{
    public void CanIndicateSuccessfullyParsedValue()
    {
        var parsed = new Parsed<string>("parsed");
        parsed.Success.ShouldBe(true);
        parsed.Value.ShouldBe("parsed");
    }

    public void ThrowsWhenAttemptingToGetFailedExpectation()
    {
        var inspectExpectation = () =>new Parsed<string>("parsed").Expectation;
        inspectExpectation
            .ShouldThrow<MemberAccessException>()
            .Message.ShouldBe("Cannot access Expectation for a Parsed reply.");
    }
}
