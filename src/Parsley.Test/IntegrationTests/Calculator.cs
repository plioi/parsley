using System;
using System.Linq;
using NUnit.Framework;

namespace Parsley.IntegrationTests
{
    [TestFixture]
    public class Calculator
    {
        [Test]
        public void Constant()
        {
            const string input = "1";
            var tokens = new CalculatorLexer(input);
            CalculatorGrammar.Expression.Parses(tokens).IntoValue(expression => expression.Value.ShouldEqual(1));
        }

        [Test]
        public void Add()
        {
            const string input = "1+2";
            var tokens = new CalculatorLexer(input);
            CalculatorGrammar.Expression.Parses(tokens).IntoValue(expression => expression.Value.ShouldEqual(3));
        }

        [Test]
        public void Subtract()
        {
            const string input = "3-1";
            var tokens = new CalculatorLexer(input);
            CalculatorGrammar.Expression.Parses(tokens).IntoValue(expression => expression.Value.ShouldEqual(2));
        }

        [Test]
        public void Multiply()
        {
            const string input = "2*3";
            var tokens = new CalculatorLexer(input);
            CalculatorGrammar.Expression.Parses(tokens).IntoValue(expression => expression.Value.ShouldEqual(6));
        }

        [Test]
        public void Divide()
        {
            const string input = "10/2";
            var tokens = new CalculatorLexer(input);
            CalculatorGrammar.Expression.Parses(tokens).IntoValue(expression => expression.Value.ShouldEqual(5));
        }

        [Test]
        public void OrderOfOperations()
        {
            const string input = "1+4/2-3*4";
            var tokens = new CalculatorLexer(input);
            CalculatorGrammar.Expression.Parses(tokens).IntoValue(expression => expression.Value.ShouldEqual(-9));
        }

        [Test]
        public void ParenthesizedSubexpressions()
        {
            const string input = "(1+4)/(2-3)*4";
            var tokens = new CalculatorLexer(input);
            CalculatorGrammar.Expression.Parses(tokens).IntoValue(expression => expression.Value.ShouldEqual(-20));
        }

        private interface Expression
        {
            int Value { get; }
        }

        private class ConstantExpression : Expression
        {
            public ConstantExpression(int value)
            {
                Value = value;
            }

            public int Value { get; private set; }
        }

        private class BinaryExpression : Expression
        {
            private readonly Expression left;
            private readonly Expression right;
            private readonly Func<int, int, int> operation;

            public BinaryExpression(Expression left, Expression right, Func<int, int, int> operation)
            {
                this.left = left;
                this.operation = operation;
                this.right = right;
            }

            public int Value
            {
                get { return operation(left.Value, right.Value); }
            }
        }

        private class CalculatorLexer : Lexer
        {
            public static readonly TokenKind Constant = new TokenKind("constant", @"[1-9][0-9]*");

            public CalculatorLexer(string source)
                : base(new Text(source),
                    Constant,
                    new Operator("("), new Operator(")"),
                    new Operator("*"), new Operator("/"),
                    new Operator("+"), new Operator("-")) { }
        }

        private class CalculatorGrammar : Grammar
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
}
