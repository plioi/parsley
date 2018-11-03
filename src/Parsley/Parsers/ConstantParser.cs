namespace Parsley.Parsers
{
    public class ConstantParser<T> : Parser<T>
    {
        public ConstantParser(TokenKind kind, T value)
        {
            _kind = kind;
            _value = value;
        }

        public override IReply<T> Parse(TokenStream tokens)
        {
            if (tokens.Current.Kind == _kind)
                return new Parsed<T>(_value, tokens.Advance());

            return new Error<T>(tokens, ErrorMessage.Expected(_kind.Name));
        }

        public override IReplyG ParseG(TokenStream tokens)
        {
            if (tokens.Current.Kind == _kind)
                return new ParsedG(tokens.Advance());

            return new ErrorG(tokens, ErrorMessage.Expected(_kind.Name));
        }

        protected override string GetName() => $"<C {_kind} := {_value}>";

        private readonly TokenKind _kind;
        private readonly T _value;
    }
}
