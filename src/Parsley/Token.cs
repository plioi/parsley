namespace Parsley
{
    public class Token
    {
        public TokenKind Kind { get; private set; }
        public Position Position { get; private set; }
        public string Literal { get; private set; }

        public Token(TokenKind kind, Position position, string value)
        {
            Kind = kind;
            Position = position;
            Literal = value;
        }

        public override string ToString()
        {
            return string.Format("Kind: {0}, Position: {1}, Literal: {2}", Kind, Position, Literal);
        }
    }
}