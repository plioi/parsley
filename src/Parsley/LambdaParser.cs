namespace Parsley;

public class LambdaParser<T> : IParser<T>
{
    private readonly Func<TokenStream, Reply<T>> parse;

    public LambdaParser(Func<TokenStream, Reply<T>> parse)
    {
        this.parse = parse;
    }

    public Reply<T> Parse(TokenStream tokens)
    {
        return parse(tokens);
    }
}
