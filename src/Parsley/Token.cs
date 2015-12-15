namespace Parsley
{
    public class Token
    {
        public TokenKind Kind { get; }
        public Position Position { get; }
        public string Literal { get; }

        public Token(TokenKind kind, Position position, string value)
        {
            Kind = kind;
            Position = position;
            Literal = value;
        }

        public override string ToString()
        {
            return $"Kind: {Kind}, Position: {Position}, Literal: {Literal}";
        }
    }
}