namespace Parsley;

public class GrammarRule<T> : IParser<T>
{
    Func<TokenStream, Reply<T>> parse;

    public GrammarRule(string name = null)
    {
        Name = name;
        parse = tokens => new Error<T>(tokens, new UndefinedGrammarRuleErrorMessage(Name));
    }

    public string Name { get; internal set; }

    public IParser<T> Rule
    {
        set { parse = value.Parse; }
    }

    public Reply<T> Parse(TokenStream tokens)
        => parse(tokens);
}
