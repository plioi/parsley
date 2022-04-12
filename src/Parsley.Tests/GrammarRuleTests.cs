using static Parsley.Grammar;

namespace Parsley.Tests;

class GrammarRuleTests
{
    public void CanDefineMutuallyRecursiveRules()
    {
        var input = "(A)";
        var letter = Pattern("Letter", @"[a-zA-Z]");
        var expression = new GrammarRule<string>();
        var alpha = new GrammarRule<string>();
        var parenthesizedExpresion = new GrammarRule<string>();

        expression.Rule = Choice(alpha, parenthesizedExpresion);
        alpha.Rule = letter;
        parenthesizedExpresion.Rule =
            from open in Operator("(")
            from expr in expression
            from close in Operator(")")
            select expr;

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
