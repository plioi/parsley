namespace Parsley.Parsers
{
    public class FailingParser<T> : Parser<T>
    {
        public override IReply<T> Parse(TokenStream tokens) => new Error<T>(tokens, ErrorMessage.Unknown());

        /// <summary>
        /// Parsing optimized for the case when the reply value is not needed.
        /// </summary>
        /// <param name="tokens">Tokens to parse</param>
        public override IReplyG ParseG(TokenStream tokens) => new ErrorG(tokens, ErrorMessage.Unknown());

        protected override string GetName() => "<FAIL>";
    }
}