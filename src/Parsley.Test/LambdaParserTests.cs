using Xunit;

namespace Parsley
{
    public class LambdaParserTests
    {
        [Fact]
        public void CreatesParsersFromLambdas()
        {
            var succeeds = new LambdaParser<string>(tokens => new Parsed<string>("AA", tokens.Advance().Advance()));
            succeeds.PartiallyParses(new CharLexer("AABB"), "BB").IntoValue("AA");

            var fails = new LambdaParser<string>(tokens => new Error<string>(tokens, ErrorMessage.Unknown()));
            fails.FailsToParse(new CharLexer("AABB"), "AABB").WithMessage("(1, 1): Parse error.");
        }
    }
}