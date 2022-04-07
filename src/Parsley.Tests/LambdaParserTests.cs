namespace Parsley.Tests;

class LambdaParserTests
{
    public void CreatesParsersFromLambdas()
    {
        var succeeds = new LambdaParser<string>(input => new Parsed<string>("AA", input.Advance().Advance()));
        succeeds.PartiallyParses(new CharLexer().Tokenize("AABB")).LeavingUnparsedTokens("B", "B").WithValue("AA");

        var fails = new LambdaParser<string>(input => new Error<string>(input, ErrorMessage.Unknown()));
        fails.FailsToParse(new CharLexer().Tokenize("AABB")).LeavingUnparsedTokens("A", "A", "B", "B").WithMessage("(1, 1): Parse error.");
    }
}
