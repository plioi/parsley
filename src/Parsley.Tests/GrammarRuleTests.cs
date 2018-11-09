using Shouldly;
using Xunit;

namespace Parsley.Tests
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
            alpha.Rule = CharLexer.Char.Literal();
            parenthesizedExpresion.Rule = Between(CharLexer.LeftParen.Kind(), expression, CharLexer.RightParen.Kind());

            expression.Parses(tokens).WithValue("A");
        }

        [Fact]
        public void HasAnOptionallyProvidedName()
        {
            var unnamed = new GrammarRule<string>();
            var named = new GrammarRule<string>("Named");

            unnamed.Name.ShouldBeNull();
            named.Name.ShouldBe("Named");
        }
    }
}
