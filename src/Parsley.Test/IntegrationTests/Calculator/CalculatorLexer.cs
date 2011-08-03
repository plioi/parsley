namespace Parsley.Test.IntegrationTests.Calculator
{
    public class CalculatorLexer : Lexer
    {
        public static readonly TokenKind Constant = new TokenKind("constant", @"[1-9][0-9]*");

        public CalculatorLexer(string source)
            : base(new Text(source),
                   Constant,
                   new Operator("("), new Operator(")"),
                   new Operator("*"), new Operator("/"),
                   new Operator("+"), new Operator("-")) { }
    }
}