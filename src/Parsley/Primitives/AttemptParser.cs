namespace Parsley.Primitives
{
    internal class AttemptParser<T> : Parser<T>
    {
        private readonly Parser<T> parse;

        public AttemptParser(Parser<T> parse)
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