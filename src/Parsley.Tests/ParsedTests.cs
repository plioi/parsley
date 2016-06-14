namespace Parsley.Tests
{
    using Shouldly;
    using Xunit;
    using ErrorMessage = Parsley.ErrorMessage;

    public class ParsedTests
    {
        readonly TokenStream unparsed;

        public ParsedTests()
        {
            unparsed = new TokenStream(new CharLexer().Tokenize("0"));
        }

        [Fact]
        public void HasAParsedValue()
        {
            new Parsed<string>("parsed", unparsed).Value.ShouldBe("parsed");
        }

        [Fact]
        public void HasNoErrorMessageByDefault()
        {
            new Parsed<string>("x", unparsed).ErrorMessages.ShouldBe(ErrorMessageList.Empty);
        }

        [Fact]
        public void CanIndicatePotentialErrors()
        {
            var potentialErrors = ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"));

            new Parsed<object>("x", unparsed, potentialErrors).ErrorMessages.ShouldBe(potentialErrors);
        }

        [Fact]
        public void HasRemainingUnparsedTokens()
        {
            new Parsed<string>("parsed", unparsed).UnparsedTokens.ShouldBe(unparsed);
        }

        [Fact]
        public void ReportsNonerrorState()
        {
            new Parsed<string>("parsed", unparsed).Success.ShouldBeTrue();
        }
    }
}