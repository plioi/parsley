using NUnit.Framework;

namespace Parsley.Test.IntegrationTests.Calculator
{
    [TestFixture]
    public class CalculatorGrammarTests : CalculatorGrammar
    {
        [Test]
        public void Constant()
        {
            var tokens = new CalculatorLexer("1");
            Expression.Parses(tokens).IntoValue(expression => expression.Value.ShouldEqual(1));
        }

        [Test]
        public void Add()
        {
            var tokens = new CalculatorLexer("1+2");
            Expression.Parses(tokens).IntoValue(expression => expression.Value.ShouldEqual(3));
        }

        [Test]
        public void Subtract()
        {
            var tokens = new CalculatorLexer("3-1");
            Expression.Parses(tokens).IntoValue(expression => expression.Value.ShouldEqual(2));
        }

        [Test]
        public void Multiply()
        {
            var tokens = new CalculatorLexer("2*3");
            Expression.Parses(tokens).IntoValue(expression => expression.Value.ShouldEqual(6));
        }

        [Test]
        public void Divide()
        {
            var tokens = new CalculatorLexer("10/2");
            Expression.Parses(tokens).IntoValue(expression => expression.Value.ShouldEqual(5));
        }

        [Test]
        public void OrderOfOperations()
        {
            var tokens = new CalculatorLexer("1+4/2-3*4");
            Expression.Parses(tokens).IntoValue(expression => expression.Value.ShouldEqual(-9));
        }

        [Test]
        public void ParenthesizedSubexpressions()
        {
            var tokens = new CalculatorLexer("(1+4)/(2-3)*4");
            Expression.Parses(tokens).IntoValue(expression => expression.Value.ShouldEqual(-20));
        }
    }
}
