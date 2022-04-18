namespace Parsley.Tests;

class ErrorTests
{
    public void CanIndicateMissedExpectations()
    {
        var error = new Error<object>();
        error.Success.ShouldBe(false);
        error.Expectation.ShouldBe("");

        error = new Error<object>("statement");
        error.Success.ShouldBe(false);
        error.Expectation.ShouldBe("statement");
    }

    public void CanIndicateMultipleMissedExpectations()
    {
        var error = new Error<object>(new[] { "A", "B" });
       error.Success.ShouldBe(false);
       error.Expectation.ShouldBe("(A or B)");
    }

    public void ThrowsWhenAttemptingToGetParsedValue()
    {
        var inspectParsedValue = () => new Error<object>().Value;
        inspectParsedValue
            .ShouldThrow<MemberAccessException>()
            .Message.ShouldBe("Cannot access Value for an Error reply.");
    }
}
