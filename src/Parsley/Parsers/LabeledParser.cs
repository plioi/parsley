namespace Parsley.Parsers
{
    public class LabeledParser<T> : Parser<T>
    {
        public LabeledParser(IParser<T> parser, string expectation)
        {
            _parser = parser;
            _errors = ErrorMessageList.Empty.With(ErrorMessage.Expected(expectation));
        }

        public override IReply<T> Parse(TokenStream tokens)
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

        protected override string GetName() => $"<LABEL {_parser.Name} WITH {_errors}";

        private readonly IParser<T> _parser;
        private readonly ErrorMessageList _errors;
    }
}
