namespace Parsley
{
    public class CharLexer : Lexer
    {
        public CharLexer()
            : base(new Pattern("Character", @".")) { }
    }
}