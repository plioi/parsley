namespace Parsley.Parsers
{
    public class MonadicUnitParser<T> : Parser<T>
    {
        public MonadicUnitParser(T value)
        {
            _value = value;
        }

        public override IReply<T> Parse(TokenStream tokens) => new Parsed<T>(_value, tokens);

        private readonly T _value;

        protected override string GetName() => $"<UNIT {_value}>";
    }
}
