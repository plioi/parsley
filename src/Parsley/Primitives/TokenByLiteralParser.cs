namespace Parsley.Primitives;

class TokenByLiteralParser : IParser<Token>
{
    readonly string expectation;

    public TokenByLiteralParser(string expectation)
    {
        this.expectation = expectation;
    }

    public Reply<Token> Parse(Input input)
    {
        if (input.Current.Literal == expectation)
            return new Parsed<Token>(input.Current, input.Advance());

        return new Error<Token>(input, ErrorMessage.Expected(expectation));
    }
}
