namespace Parsley.Tests;

class LambdaParserTests
{
    public void CreatesParsersFromLambdas()
    {
        var succeeds = new LambdaParser<string>(input => new Parsed<string>("AA", input.Advance(2)));
        succeeds.PartiallyParses("AABB").LeavingUnparsedInput("BB").WithValue("AA");

        var fails = new LambdaParser<string>(input => new Error<string>(input, ErrorMessage.Unknown()));
        fails.FailsToParse("AABB").LeavingUnparsedInput("AABB").WithMessage("(1, 1): Parse error.");
    }
}
