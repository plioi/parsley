using System.Globalization;
using static Parsley.Grammar;

namespace Parsley.Tests;

class OperatorPrecedenceParserTests
{
    readonly OperatorPrecedenceParser<IExpression> expression;

    public OperatorPrecedenceParserTests()
    {
        expression = new OperatorPrecedenceParser<IExpression>();

        expression.Atom(Digit, token => new Constant(int.Parse(token, CultureInfo.InvariantCulture)));
        expression.Atom(Name, token => new Identifier(token));

        expression.Unit(LeftParen,
            from open in LeftParen
            from expr in expression.Parser
            from close in RightParen
            select expr);

        expression.Postfix(Increment, 7, (symbol, operand) => new Form(new Identifier(symbol), operand));
        expression.Postfix(Decrement, 7, (symbol, operand) => new Form(new Identifier(symbol), operand));
        expression.Binary(Add, 3, (left, symbol, right) => new Form(symbol, left, right));
        expression.Binary(Subtract, 3, (left, symbol, right) => new Form(symbol, left, right));
        expression.Binary(Multiply, 4, (left, symbol, right) => new Form(symbol, left, right));
        expression.Binary(Divide, 4, (left, symbol, right) => new Form(symbol, left, right));
        expression.Binary(Exponent, 5, (left, symbol, right) => new Form(symbol, left, right), Associativity.Right);
        expression.Prefix(Subtract, 6, (symbol, operand) => new Form(new Identifier(symbol), operand));

        expression.Extend(LeftParen, 8, callable =>
            from open in LeftParen
            from arguments in ZeroOrMore(expression.Parser, Comma)
            from close in RightParen
            select new Form(callable, arguments));
    }

    public void ParsesRegisteredTokensIntoCorrespondingAtoms()
    {
        Parses("1", "1");
        Parses("square", "square");
    }

    public void ParsesUnitExpressionsStartedByRegisteredTokens()
    {
        Parses("(0)", "0");
        Parses("(square)", "square");
        Parses("(1+4)/(2-3)*4", "(* (/ (+ 1 4) (- 2 3)) 4)");
    }

    public void ParsesPrefixExpressionsStartedByRegisteredToken()
    {
        Parses("-1", "(- 1)");
        Parses("-(-1)", "(- (- 1))");
    }

    public void ParsesPostfixExpressionsEndedByRegisteredToken()
    {
        Parses("1++", "(++ 1)");
        Parses("1++--", "(-- (++ 1))");
    }

    public void ParsesExpressionsThatExtendTheLeftSideExpressionWhenTheRegisteredTokenIsEncountered()
    {
        Parses("square(1)", "(square 1)");
        Parses("square(1,2)", "(square 1 2)");
    }

    public void ParsesBinaryOperationsRespectingPrecedenceAndAssociativity()
    {
        Parses("1+2", "(+ 1 2)");
        Parses("1-2", "(- 1 2)");
        Parses("1*2", "(* 1 2)");
        Parses("1/2", "(/ 1 2)");
        Parses("1^2", "(^ 1 2)");

        Parses("1+2+3", "(+ (+ 1 2) 3)");
        Parses("1-2-3", "(- (- 1 2) 3)");
        Parses("1*2*3", "(* (* 1 2) 3)");
        Parses("1/2/3", "(/ (/ 1 2) 3)");
        Parses("1^2^3", "(^ 1 (^ 2 3))");

        Parses("1*2/3-4", "(- (/ (* 1 2) 3) 4)");
        Parses("1/2*3-4", "(- (* (/ 1 2) 3) 4)");
        Parses("1+2-3*4", "(- (+ 1 2) (* 3 4))");
        Parses("1-2+3*4", "(+ (- 1 2) (* 3 4))");
        Parses("1^2^3*4", "(* (^ 1 (^ 2 3)) 4)");
        Parses("1^2^3*4", "(* (^ 1 (^ 2 3)) 4)");
        Parses("1*2/3^4", "(/ (* 1 2) (^ 3 4))");
        Parses("1^2+3^4", "(+ (^ 1 2) (^ 3 4))");
    }

    public void ProvidesErrorAtAppropriatePositionWhenUnitParsersFail()
    {
        //Upon unit-parser failures, stop!
            
        //The "(" unit-parser is invoked but fails.  The next token, "*", has
        //high precedence, but that should not provoke parsing to continue.
            
        expression.Parser.FailsToParse("(*", "*", "expression expected");
    }

    public void ProvidesErrorAtAppropriatePositionWhenExtendParsersFail()
    {
        //Upon extend-parser failures, stop!
            
        //The "2" unit-parser succeeds.  The next token, "-" has
        //high-enough precedence to continue, so the "-" extend-parser
        //is invoked and immediately fails.  The next token, "*", has
        //high precedence, but that should not provoke parsing to continue.

        expression.Parser.FailsToParse("2-*", "*", "expression expected");
    }

    void Parses(string input, string expectedTree)
    {
        var value = expression.Parser.Parses(input).Value;
        value.ToString().ShouldBe(expectedTree);
    }

    static readonly Parser<string> Digit = from c in Character(char.IsDigit, "Digit") select c.ToString();
    static readonly Parser<string> Name = OneOrMore(char.IsLetter, "Name");
    static readonly Parser<string> Increment = Operator("++");
    static readonly Parser<string> Decrement = Operator("--");
    static readonly Parser<string> Add = Operator("+");
    static readonly Parser<string> Subtract = Operator("-");
    static readonly Parser<string> Multiply = Operator("*");
    static readonly Parser<string> Divide = Operator("/");
    static readonly Parser<string> Exponent = Operator("^");
    static readonly Parser<string> LeftParen = Operator("(");
    static readonly Parser<string> RightParen = Operator(")");
    static readonly Parser<string> Comma = Operator(",");

    interface IExpression
    {
    }

    class Constant : IExpression
    {
        readonly int value;

        public Constant(int value)
            => this.value = value;

        public override string ToString()
            => value.ToString(CultureInfo.InvariantCulture);
    }

    class Identifier : IExpression
    {
        readonly string identifier;

        public Identifier(string identifier)
            => this.identifier = identifier;

        public override string ToString()
            => identifier;
    }

    class Form : IExpression
    {
        readonly IExpression head;
        readonly IEnumerable<IExpression> expressions;

        public Form(string head, params IExpression[] expressions)
            : this(new Identifier(head), expressions) { }

        public Form(IExpression head, params IExpression[] expressions)
            : this(head, (IEnumerable<IExpression>)expressions) { }

        public Form(IExpression head, IEnumerable<IExpression> expressions)
        {
            this.head = head;
            this.expressions = expressions;
        }

        public override string ToString()
            => "(" + head + " " + string.Join(" ", expressions) + ")";
    }
}
