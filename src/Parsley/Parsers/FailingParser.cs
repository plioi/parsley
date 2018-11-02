namespace Parsley.Parsers
{
    public class FailingParser<T> : Parser<T>
    {
        public override IReply<T> Parse(TokenStream tokens)
            => new Error<T>(tokens, ErrorMessage.Unknown());

        protected override string GetName() => "<FAIL>";
    }
}