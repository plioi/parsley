using Should;
using Xunit;

namespace Parsley
{
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
    }
}
