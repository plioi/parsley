namespace Parsley.Tests;

class LambdaParserTests
{
    public void CreatesParsersFromLambdas()
    {
        var succeeds = new LambdaParser<string>(tokens => new Parsed<string>("AA", tokens.Advance().Advance()));
        succeeds.PartiallyParses(new CharLexer().Tokenize("AABB")).LeavingUnparsedTokens("B", "B").WithValue("AA");

        var fails = new LambdaParser<string>(tokens => new Error<string>(tokens, ErrorMessage.Unknown()));
        fails.FailsToParse(new CharLexer().Tokenize("AABB")).LeavingUnparsedTokens("A", "A", "B", "B").WithMessage("(1, 1): Parse error.");
    }
}
