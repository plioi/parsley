namespace Parsley.Parsers
{
    public class MonadicUnitParser<T> : Parser<T>
    {
        public MonadicUnitParser(T value)
        {
            _value = value;
        }

        public override IReply<T> Parse(TokenStream tokens) => new Parsed<T>(_value, tokens);

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed.
        /// </summary>
        /// <param name="tokens">Tokens to parse</param>
        public override IGeneralReply ParseGeneral(TokenStream tokens) => new ParsedGeneral(tokens);

        private readonly T _value;

        protected override string GetName() => $"<= {_value}>";
    }
}
