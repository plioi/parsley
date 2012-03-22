namespace Parsley
{
    public class CharTokenStream : TokenStream
    {
        public CharTokenStream(string source)
            : base(new Text(source), new Pattern("Character", @".")) { }
    }
}