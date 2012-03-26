namespace Parsley
{
    public class CharTokenStream : TokenStream
    {
        public CharTokenStream(string source)
            : base(new Lexer(new Pattern("Character", @".")).Tokenize(new Text(source))) { }
    }
}