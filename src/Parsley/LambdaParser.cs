namespace Parsley;

public class LambdaParser<T> : IParser<T>
{
    readonly Func<Input, Reply<T>> parse;

    public LambdaParser(Func<Input, Reply<T>> parse)
        => this.parse = parse;

    public Reply<T> Parse(Input input)
        => parse(input);
}
