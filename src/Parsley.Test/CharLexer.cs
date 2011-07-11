namespace Parsley
{
    public class CharLexer : Lexer
    {
        public CharLexer(string source)
            : base(new Text(source), new TokenKind("Character", @".")) { }
    }
}