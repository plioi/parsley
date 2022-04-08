namespace Parsley;

public class GrammarRule<T> : IParser<T>
{
    Func<Text, Reply<T>> parse;

    public GrammarRule(string name = null)
    {
        Name = name;
        parse = input => new Error<T>(input, new UndefinedGrammarRuleErrorMessage(Name));
    }

    public string Name { get; internal set; }

    public IParser<T> Rule
    {
        set { parse = value.Parse; }
    }

    public Reply<T> Parse(Text input)
        => parse(input);
}
