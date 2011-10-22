using System;

namespace Parsley.Test.IntegrationTests.Calculator
{
    public class CalculatorGrammar : Grammar
    {
        public static readonly OperatorPrecedenceParser<Expression> Expression = new OperatorPrecedenceParser<Expression>();
        private static readonly GrammarRule<Expression> Parenthesized = new GrammarRule<Expression>();
        
        static CalculatorGrammar()
        {
            Parenthesized.Rule =
                Between(Token("("), Expression, Token(")"));

            Expression.Atom(CalculatorLexer.Constant, token => new ConstantExpression(int.Parse(token.Literal)));
            Expression.Unit(CalculatorLexer.LeftParen, Parenthesized);
            Expression.Binary(CalculatorLexer.Multiply, 2, Node((x, y) => x * y));
            Expression.Binary(CalculatorLexer.Divide, 2, Node((x, y) => x / y));
            Expression.Binary(CalculatorLexer.Add, 1, Node((x, y) => x + y));
            Expression.Binary(CalculatorLexer.Subtract, 1, Node((x, y) => x - y));
        }

        private static BinaryNodeBuilder<Expression> Node(Func<int, int, int> operation)
        {
            return (left, symbol, right) => new BinaryExpression(left, right, operation);
        }
    }
}
