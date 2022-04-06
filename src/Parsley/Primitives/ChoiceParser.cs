namespace Parsley.Primitives
{
    internal class ChoiceParser<T> : Parser<T>
    {
        private readonly Parser<T>[] parsers;

        public ChoiceParser(Parser<T>[] parsers)
        {
            this.parsers = parsers;
        }

        public Reply<T> Parse(TokenStream tokens)
        {
            var start = tokens.Position;
            var reply = parsers[0].Parse(tokens);
            var newPosition = reply.UnparsedTokens.Position;

            var errors = ErrorMessageList.Empty;
            var i = 1;
            while (!reply.Success && (start == newPosition) && i < parsers.Length)
            {
                errors = errors.Merge(reply.ErrorMessages);
                reply = parsers[i].Parse(tokens);
                newPosition = reply.UnparsedTokens.Position;
                i++;
            }
            if (start == newPosition)
            {
                errors = errors.Merge(reply.ErrorMessages);
                if (reply.Success)
                    reply = new Parsed<T>(reply.Value, reply.UnparsedTokens, errors);
                else
                    reply = new Error<T>(reply.UnparsedTokens, errors);
            }

            return reply;
        }
    }
}