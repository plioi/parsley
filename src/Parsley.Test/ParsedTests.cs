using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class ParsedTests
    {
        private Lexer unparsed;

        [SetUp]
        public void SetUp()
        {
            unparsed = new CharLexer("0");
        }

        [Test]
        public void HasAParsedValue()
        {
            new Parsed<string>("parsed", unparsed).Value.ShouldEqual("parsed");
        }

        [Test]
        public void HasNoErrorMessageByDefault()
        {
            new Parsed<string>("x", unparsed).ErrorMessages.ShouldEqual(ErrorMessageList.Empty);
        }

        [Test]
        public void CanIndicatePotentialErrors()
        {
            var potentialErrors = ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"));

            new Parsed<object>("x", unparsed, potentialErrors).ErrorMessages.ShouldEqual(potentialErrors);
        }

        [Test]
        public void HasRemainingUnparsedTokens()
        {
            new Parsed<string>("parsed", unparsed).UnparsedTokens.ShouldEqual(unparsed);
        }

        [Test]
        public void ReportsNonerrorState()
        {
            new Parsed<string>("parsed", unparsed).Success.ShouldBeTrue();
        }
    }
}