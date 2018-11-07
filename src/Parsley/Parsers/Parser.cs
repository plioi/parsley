namespace Parsley.Parsers
{
    public abstract class Parser : IParserG
    {
        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed.
        /// </summary>
        /// <param name="tokens">Tokens to parse.</param>
        public abstract IReplyG ParseG(TokenStream tokens);

        public override string ToString() => Name;

        protected abstract string GetName();

        public string Name
        {
            get
            {
                if (_nameRecursionGuard)
                    return "<~>";

                _nameRecursionGuard = true;

                var name = GetName();

                _nameRecursionGuard = false;

                return name;
            }
        }

        private bool _nameRecursionGuard;
    }

    public abstract class Parser<T> : Parser, IParser<T>
    {
        public abstract IReply<T> Parse(TokenStream tokens);

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed.
        /// </summary>
        /// <param name="tokens">Tokens to parse.</param>
        public override IReplyG ParseG(TokenStream tokens)
        {
            return Parse(tokens);
        }
    }
}
