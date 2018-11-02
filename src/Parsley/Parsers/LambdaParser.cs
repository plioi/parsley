using System;

namespace Parsley.Parsers
{
    public class LambdaParser<T> : Parser<T>
    {
        private readonly Func<TokenStream, IReply<T>> _parse;

        public LambdaParser(Func<TokenStream, IReply<T>> parse)
        {
            _parse = parse;
        }

        public override IReply<T> Parse(TokenStream tokens)
        {
            return _parse(tokens);
        }

        protected override string GetName() => $"<L {typeof(T)}>";
    }
}