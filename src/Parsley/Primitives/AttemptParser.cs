namespace Parsley.Primitives
{
    public class AttemptParser<T> : IParser<T>
    {
        private readonly IParser<T> _parse;

        public AttemptParser(IParser<T> parse)
        {
            _parse = parse;
        }

        public string Name => $"<ATTEMPT {_parse.Name}>";

        public Reply<T> Parse(TokenStream tokens)
        {
            var start = tokens.Position;
            var reply = _parse.Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            if (reply.Success || start == newPosition)
                return reply;

            return new Error<T>(tokens, ErrorMessage.Backtrack(newPosition, reply.ErrorMessages));
        }
    }
}