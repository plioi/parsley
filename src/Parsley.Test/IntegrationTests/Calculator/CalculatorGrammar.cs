using System;
using System.Linq;

namespace Parsley.Test.IntegrationTests.Calculator
{
    public class CalculatorGrammar : Grammar
    {
        public static readonly GrammarRule<Expression> Expression = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> Constant = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> Parenthesized = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> Operand = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> Multiplicative = new GrammarRule<Expression>();
        private static readonly GrammarRule<Expression> Additive = new GrammarRule<Expression>();
        
        static CalculatorGrammar()
        {
            Constant.Rule =
                from token in Token(CalculatorLexer.Constant)
                select new ConstantExpression(int.Parse(token.Literal));

            Parenthesized.Rule =
                Between(Token("("), Expression, Token(")"));

            Operand.Rule =
                Choice(Constant, Parenthesized);

            Multiplicative.Rule =
                Binary(Operand, "*", "/");

            Additive.Rule =
                Binary(Multiplicative, "+", "-");

            Expression.Rule =
                Additive;
        }

        private static Parser<Expression> Binary(Parser<Expression> operand, params string[] symbols)
        {
            var symbolParsers = symbols.Select(Token).ToArray();

            return LeftAssociative(operand, Choice(symbolParsers),
                                   (left, symbolAndRight) =>
                                   new BinaryExpression(left, symbolAndRight.Item2,
                                                        Operation(symbolAndRight.Item1.Literal)));
        }

        private static Func<int, int, int> Operation(string symbol)
        {
            switch (symbol)
            {
                case "*": return (x, y) => x * y;
                case "/": return (x, y) => x / y;
                case "+": return (x, y) => x + y;
                case "-": return (x, y) => x - y;
                default: return (x, y) => { throw new ArgumentException("Unrecognized operator: " + symbol); };
            }
        }
    }
}
