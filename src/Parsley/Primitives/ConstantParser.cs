namespace Parsley.Primitives
{
    public class ConstantParser<T> : IParser<T>
    {
        private readonly TokenKind _kind;
        private readonly T _value;

        public ConstantParser(TokenKind kind, T value)
        {
            _kind = kind;
            _value = value;
        }

        public Reply<T> Parse(TokenStream tokens)
        {
            if (tokens.Current.Kind == _kind)
                return new Parsed<T>(_value, tokens.Advance());

            return new Error<T>(tokens, ErrorMessage.Expected(_kind.Name));
        }

        public override string ToString() => $"<CONST {_kind} RETURNS {_value}>";
    }
}
