using static Parsley.Grammar;

namespace Parsley.Tests;

class GrammarRuleTests
{
    public void CanDefineMutuallyRecursiveRules()
    {
        var input = "(A)";
        var open = new Operator("(");
        var letter = new Pattern("Letter", @"[a-zA-Z]");
        var close = new Operator(")");
        var expression = new GrammarRule<string>();
        var alpha = new GrammarRule<string>();
        var parenthesizedExpresion = new GrammarRule<string>();

        expression.Rule = Choice(alpha, parenthesizedExpresion);
        alpha.Rule = from a in letter select a.Literal;
        parenthesizedExpresion.Rule = Between(open, expression, close);

        expression.Parses(input).WithValue("A");
    }

    public void HasAnOptionallyProvidedName()
    {
        var unnamed = new GrammarRule<string>();
        var named = new GrammarRule<string>("Named");

        unnamed.Name.ShouldBeNull();
        named.Name.ShouldBe("Named");
    }

    public void ProvidesAdviceWhenRuleIsUsedBeforeBeingInitialized()
    {
        var numeric = new GrammarRule<string>();
        var alpha = new GrammarRule<string>("Alpha");

        numeric.FailsToParse("123").WithMessage("(1, 1): An anonymous GrammarRule has not been initialized.  Try setting the Rule property.");
        alpha.FailsToParse("123").WithMessage("(1, 1): GrammarRule 'Alpha' has not been initialized.  Try setting the Rule property.");
    }
}
