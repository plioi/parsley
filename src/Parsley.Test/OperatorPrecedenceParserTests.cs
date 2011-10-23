using System.Collections.Generic;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class OperatorPrecedenceParserTests : Grammar
    {
        private OperatorPrecedenceParser<Expression> expression;

        [SetUp]
        public void SetUp()
        {
            expression = new OperatorPrecedenceParser<Expression>();

            expression.Atom(SampleLexer.Digit, token => new Constant(int.Parse(token.Literal)));
            expression.Atom(SampleLexer.Name, token => new Identifier(token.Literal));

            expression.Unit(SampleLexer.LeftParen, Between(Token("("), expression, Token(")")));

            expression.Prefix(SampleLexer.Subtract, 10, (symbol, operand) => new Form(new Identifier(symbol.Literal), operand));

            expression.Extend(SampleLexer.LeftParen, 12, callable =>
                                                     from arguments in Between(Token("("), ZeroOrMore(expression, Token(",")), Token(")"))
                                                     select new Form(callable, arguments));

            expression.Binary(SampleLexer.Multiply, 9, (left, symbol, right) => new Form(symbol, left, right));
            expression.Binary(SampleLexer.Divide, 9, (left, symbol, right) => new Form(symbol, left, right));
            expression.Binary(SampleLexer.Add, 8, (left, symbol, right) => new Form(symbol, left, right));
            expression.Binary(SampleLexer.Subtract, 8, (left, symbol, right) => new Form(symbol, left, right));
        }

        [Test]
        public void ParsesRegisteredTokensIntoCorrespondingAtoms()
        {
            Parses("1", "1");
            Parses("square", "square");
        }

        [Test]
        public void ParsesUnitExpressionsStartedByRegisteredTokens()
        {
            Parses("(0)", "0");
            Parses("(square)", "square");
            Parses("(1+4)/(2-3)*4", "(* (/ (+ 1 4) (- 2 3)) 4)");
        }

        [Test]
        public void ParsesPrefixExpressionsStartedByRegisteredToken()
        {
            Parses("-1", "(- 1)");
            Parses("--1", "(- (- 1))");
        }

        [Test]
        public void ParsesExpressionsThatExtendTheLeftSideExpressionWhenTheRegisteredTokenIsEncountered()
        {
            Parses("square(1)", "(square 1)");
            Parses("square(1,2)", "(square 1 2)");
        }

        [Test]
        public void ParsesLeftAssociativeBinaryOperationsRespectingPrecedence()
        {
            Parses("1*2", "(* 1 2)");
            Parses("1/2", "(/ 1 2)");
            Parses("1+2", "(+ 1 2)");
            Parses("1-2", "(- 1 2)");

            Parses("1*2*3", "(* (* 1 2) 3)");
            Parses("1/2/3", "(/ (/ 1 2) 3)");
            Parses("1+2+3", "(+ (+ 1 2) 3)");
            Parses("1-2-3", "(- (- 1 2) 3)");

            Parses("1*2/3-4", "(- (/ (* 1 2) 3) 4)");
            Parses("1/2*3-4", "(- (* (/ 1 2) 3) 4)");
            Parses("1+2-3*4", "(- (+ 1 2) (* 3 4))");
            Parses("1-2+3*4", "(+ (- 1 2) (* 3 4))");
        }

        [Test]
        public void ProvidesErrorAtAppropriatePositionWhenUnitParsersFail()
        {
            //Upon unit-parser failures, stop!
            
            //The "(" unit-parser is invoked but fails.  The next token, "*", has
            //high precedence, but that should not provoke parsing to continue.
            
            expression.FailsToParse(Tokenize("(*"), "*").WithMessage("(1, 2): Parse error.");
        }

        [Test]
        public void ProvidesErrorAtAppropriatePositionWhenExtendParsersFail()
        {
            //Upon extend-parser failures, stop!
            
            //The "2" unit-parser succeeds.  The next token, "-" has
            //high-enough precedence to continue, so the "-" extend-parser
            //is invoked and immediately fails.  The next token, "*", has
            //high precedence, but that should not provoke parsing to continue.

            expression.FailsToParse(Tokenize("2-*"), "*").WithMessage("(1, 3): Parse error.");
        }

        private void Parses(string input, string expectedTree)
        {
            expression.Parses(Tokenize(input)).IntoValue(e => e.ToString().ShouldEqual(expectedTree));
        }

        private static Lexer Tokenize(string source)
        {
            return new SampleLexer(source);
        }

        private class SampleLexer : Lexer
        {
            public static readonly TokenKind Digit = new TokenKind("Digit", @"[0-9]");
            public static readonly TokenKind Name = new TokenKind("Name", @"[a-z]+");
            public static readonly TokenKind Add = new Operator("+");
            public static readonly TokenKind Subtract = new Operator("-");
            public static readonly TokenKind Multiply = new Operator("*");
            public static readonly TokenKind Divide = new Operator("/");
            public static readonly TokenKind LeftParen = new Operator("(");
            public static readonly TokenKind RightParen = new Operator(")");
            public static readonly TokenKind Comma = new Operator(",");

            public SampleLexer(string source)
                : base(new Text(source), Digit, Name, Add, Subtract, Multiply, Divide, LeftParen, RightParen, Comma) { }
        }

        private interface Expression
        {
        }

        private class Constant : Expression
        {
            private readonly int value;

            public Constant(int value)
            {
                this.value = value;
            }

            public override string ToString()
            {
                return value.ToString();
            }
        }

        private class Identifier : Expression
        {
            private readonly string identifier;

            public Identifier(string identifier)
            {
                this.identifier = identifier;
            }

            public override string ToString()
            {
                return identifier;
            }
        }

        private class Form : Expression
        {
            private readonly Expression head;
            private readonly IEnumerable<Expression> expressions;

            public Form(Token head, params Expression[] expressions)
                : this(new Identifier(head.Literal), expressions) { }

            public Form(Expression head, params Expression[] expressions)
                : this(head, (IEnumerable<Expression>)expressions) { }

            public Form(Expression head, IEnumerable<Expression> expressions)
            {
                this.head = head;
                this.expressions = expressions;
            }

            public override string ToString()
            {
                return "(" + head + " " + string.Join(" ", expressions) + ")";
            }
        }
    }
}