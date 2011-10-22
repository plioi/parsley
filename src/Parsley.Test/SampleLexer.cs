namespace Parsley
{
    public class SampleLexer : Lexer
    {
        public static readonly TokenKind Digit = new TokenKind("Digit", @"[0-9]");
        public static readonly TokenKind Letter = new TokenKind("Letter", @"[a-zA-Z]");
        public static readonly TokenKind Not = new Operator("!");
        public static readonly TokenKind Subtract = new Operator("-");
        public static readonly TokenKind Multiply = new Operator("*");
        public static readonly TokenKind LeftBrace = new Operator("[");
        public static readonly TokenKind RightBrace = new Operator("]");
        public static readonly TokenKind LeftParen = new Operator("(");
        public static readonly TokenKind RightParen = new Operator(")");
        public static readonly TokenKind Symbol = new TokenKind("Symbol", @".");

        public SampleLexer(string source)
            : base(new Text(source), Digit, Letter, Not, Subtract, Multiply, LeftBrace, RightBrace, LeftParen, RightParen, Symbol) { }
    }
}