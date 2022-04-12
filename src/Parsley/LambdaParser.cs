namespace Parsley;

public class LambdaParser<T> : Parser<T>
{
    readonly Func<Text, Reply<T>> parse;

    public LambdaParser(Func<Text, Reply<T>> parse)
        => this.parse = parse;

    public Reply<T> Parse(Text input)
        => parse(input);
}
