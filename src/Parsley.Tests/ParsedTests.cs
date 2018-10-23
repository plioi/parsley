namespace Parsley.Tests
{
    using Shouldly;
    using Xunit;
    
    public class ParsedTests
    {
        private readonly TokenStream _unparsed = new TokenStream(new CharLexer().Tokenize("0"));

        [Fact]
        public void HasAParsedValue()
        {
            new Parsed<string>("parsed", _unparsed).Value.ShouldBe("parsed");
        }

        [Fact]
        public void HasNoErrorMessageByDefault()
        {
            new Parsed<string>("x", _unparsed).ErrorMessages.ShouldBe(ErrorMessageList.Empty);
        }

        [Fact]
        public void CanIndicatePotentialErrors()
        {
            var potentialErrors = ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"));

            new Parsed<object>("x", _unparsed, potentialErrors).ErrorMessages.ShouldBe(potentialErrors);
        }

        [Fact]
        public void HasRemainingUnparsedTokens()
        {
            new Parsed<string>("parsed", _unparsed).UnparsedTokens.ShouldBe(_unparsed);
        }

        [Fact]
        public void ReportsNonerrorState()
        {
            new Parsed<string>("parsed", _unparsed).Success.ShouldBeTrue();
        }
    }
}