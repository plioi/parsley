namespace Parsley.Primitives;

class EndOfInputParser : IParser<string>
{
    public Reply<string> Parse(Text input)
        => input.EndOfInput
            ? new Parsed<string>("", input)
            : new Error<string>(input, ErrorMessage.Expected("end of input"));
}
