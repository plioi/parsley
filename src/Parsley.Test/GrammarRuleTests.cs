using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class GrammarRuleTests : Grammar
    {
        [Test]
        public void CanDefineMutuallyRecursiveRules()
        {
            var tokens = new CharLexer("(A)");
            var expression = new GrammarRule<string>();
            var alpha = new GrammarRule<string>();
            var parenthesizedExpresion = new GrammarRule<string>();

            expression.Rule = Choice(alpha, parenthesizedExpresion);
            alpha.Rule = from a in Token("A") select a.Literal;
            parenthesizedExpresion.Rule = Between(Token("("), expression, Token(")"));

            expression.Parses(tokens).IntoValue("A");
        }
    }
}
