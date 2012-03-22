using Should;
using Xunit;

namespace Parsley
{
    public class ParsedTests
    {
        private readonly TokenStream unparsed;

        public ParsedTests()
        {
            unparsed = new CharTokenStream("0");
        }

        [Fact]
        public void HasAParsedValue()
        {
            new Parsed<string>("parsed", unparsed).Value.ShouldEqual("parsed");
        }

        [Fact]
        public void HasNoErrorMessageByDefault()
        {
            new Parsed<string>("x", unparsed).ErrorMessages.ShouldEqual(ErrorMessageList.Empty);
        }

        [Fact]
        public void CanIndicatePotentialErrors()
        {
            var potentialErrors = ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"));

            new Parsed<object>("x", unparsed, potentialErrors).ErrorMessages.ShouldEqual(potentialErrors);
        }

        [Fact]
        public void HasRemainingUnparsedTokens()
        {
            new Parsed<string>("parsed", unparsed).UnparsedTokens.ShouldEqual(unparsed);
        }

        [Fact]
        public void ReportsNonerrorState()
        {
            new Parsed<string>("parsed", unparsed).Success.ShouldBeTrue();
        }
    }
}