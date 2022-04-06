namespace Parsley.Primitives
{
    internal class AttemptParser<T> : IParser<T>
    {
        private readonly IParser<T> parse;

        public AttemptParser(IParser<T> parse)
        {
            this.parse = parse;
        }

        public Reply<T> Parse(TokenStream tokens)
        {
            var start = tokens.Position;
            var reply = parse.Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            if (reply.Success || start == newPosition)
                return reply;

            return new Error<T>(tokens, ErrorMessage.Backtrack(newPosition, reply.ErrorMessages));
        }
    }
}