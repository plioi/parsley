using Should;

namespace Parsley
{
    public class ParsedTests
    {
        private readonly TokenStream unparsed;

        public ParsedTests()
        {
            unparsed = new TokenStream(new CharLexer().Tokenize("0"));
        }

        public void HasAParsedValue()
        {
            new Parsed<string>("parsed", unparsed).Value.ShouldEqual("parsed");
        }

        public void HasNoErrorMessageByDefault()
        {
            new Parsed<string>("x", unparsed).ErrorMessages.ShouldEqual(ErrorMessageList.Empty);
        }

        public void CanIndicatePotentialErrors()
        {
            var potentialErrors = ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"));

            new Parsed<object>("x", unparsed, potentialErrors).ErrorMessages.ShouldEqual(potentialErrors);
        }

        public void HasRemainingUnparsedTokens()
        {
            new Parsed<string>("parsed", unparsed).UnparsedTokens.ShouldEqual(unparsed);
        }

        public void ReportsNonerrorState()
        {
            new Parsed<string>("parsed", unparsed).Success.ShouldBeTrue();
        }
    }
}