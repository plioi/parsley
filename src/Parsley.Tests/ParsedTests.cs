namespace Parsley.Tests
{
    using Shouldly;
    using ErrorMessage = Parsley.ErrorMessage;

    public class ParsedTests
    {
        readonly TokenStream unparsed;

        public ParsedTests()
        {
            unparsed = new TokenStream(new CharLexer().Tokenize("0"));
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

        public void HasRemainingUnparsedTokens()
        {
            new Parsed<string>("parsed", unparsed).UnparsedTokens.ShouldBe(unparsed);
        }

        public void ReportsNonerrorState()
        {
            new Parsed<string>("parsed", unparsed).Success.ShouldBeTrue();
        }
    }
}
