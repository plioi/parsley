namespace Parsley
{
    using System.Linq;
    using Should;
    using Xunit;

    public class GrammarRuleTests : Grammar
    {
        [Fact]
        public void CanDefineMutuallyRecursiveRules()
        {
            var tokens = new CharLexer().Tokenize("(A)");
            var expression = new GrammarRule<string>();
            var alpha = new GrammarRule<string>();
            var parenthesizedExpresion = new GrammarRule<string>();

            expression.Rule = Choice(alpha, parenthesizedExpresion);
            alpha.Rule = from a in Token("A") select a.Literal;
            parenthesizedExpresion.Rule = Between(Token("("), expression, Token(")"));

            expression.Parses(tokens).WithValue("A");
        }

        [Fact]
        public void HasAnOptionallyProvidedName()
        {
            var unnamed = new GrammarRule<string>();
            var named = new GrammarRule<string>("Named");

            unnamed.Name.ShouldBeNull();
            named.Name.ShouldEqual("Named");
        }

        [Fact]
        public void ProvidesAdviceWhenRuleIsUsedBeforeBeingInitialized()
        {
            var tokens = new CharLexer().Tokenize("123").ToArray();
            var numeric = new GrammarRule<string>();
            var alpha = new GrammarRule<string>("Alpha");

            numeric.FailsToParse(tokens).WithMessage("(1, 1): An anonymous GrammarRule has not been initialized.  Try setting the Rule property.");
            alpha.FailsToParse(tokens).WithMessage("(1, 1): GrammarRule 'Alpha' has not been initialized.  Try setting the Rule property.");
        }
    }
}
