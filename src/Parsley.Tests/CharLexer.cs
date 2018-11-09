namespace Parsley.Tests
{
    public class CharLexer : Lexer
    {
        public static TokenKind Char = new Pattern("Character", @".");
        public static readonly TokenKind LeftParen = new Operator("(");
        public static readonly TokenKind RightParen = new Operator(")");
        public CharLexer()
            : base(LeftParen, RightParen, Char) { }
    }
}