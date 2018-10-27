namespace Parsley.Primitives
{
    public class LabeledParser<T> : IParser<T>
    {
        private readonly IParser<T> _parser;
        private readonly ErrorMessageList _errors;

        public LabeledParser(IParser<T> parser, string expectation)
        {
            _parser = parser;
            _errors = ErrorMessageList.Empty.With(ErrorMessage.Expected(expectation));
        }

        public Reply<T> Parse(TokenStream tokens)
        {
            var oldPosition = tokens.Position;
            var reply = _parser.Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            if (oldPosition != newPosition)
                return reply;

            if (reply.Success)
                return new Parsed<T>(reply.Value, reply.UnparsedTokens, _errors);
                
            return new Error<T>(reply.UnparsedTokens, _errors);
        }

        public override string ToString() => $"<LABEL {_parser.Name} WITH {_errors}";

        public string Name => ToString();
    }
}
