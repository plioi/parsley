using System;
using System.Linq;

namespace Parsley.Test.IntegrationTests.Calculator
{
    public class CalculatorGrammar : Grammar
    {
        public static Parser<Expression> Expression
        {
            get
            {
                var Multiplicative = Binary(Operand, "*", "/");
                var Additive = Binary(Multiplicative, "+", "-");
                return Additive;
            }
        }

        private static Parser<Expression> Operand
        {
            get
            {
                return Choice(Constant, Parenthesized);
            }
        }

        private static Parser<Expression> Constant
        {
            get
            {
                return from token in Token(CalculatorLexer.Constant)
                       select new ConstantExpression(int.Parse(token.Literal));
            }
        }

        private static Parser<Expression> Parenthesized
        {
            get
            {
                //NOTE: To avoid infinite recursion upon get, we cannot evaluate the 'Expression'
                //      property right now. Instead, we must wrap it in a lambda so that it won't
                //      be evaluated until the parser is actually being executed.
                //
                //      'Expression' is funcionally equivalent to 'tokens => Expression(tokens)'

                return Between(Token("("), tokens => Expression(tokens), Token(")"));
            }
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
