namespace Parsley.Test.IntegrationTests.Calculator
{
    public class CalculatorLexer : Lexer
    {
        public static readonly TokenKind Constant = new TokenKind("constant", @"[1-9][0-9]*");
        public static readonly Operator LeftParen = new Operator("(");
        public static readonly Operator RightParen = new Operator(")");
        public static readonly Operator Multiply = new Operator("*");
        public static readonly Operator Divide = new Operator("/");
        public static readonly Operator Add = new Operator("+");
        public static readonly Operator Subtract = new Operator("-");

        public CalculatorLexer(string source)
            : base(new Text(source), Constant, LeftParen, RightParen, Multiply, Divide, Add, Subtract) { }
    }
}