namespace Parsley;

partial class Grammar
{
    public static Reply<string> EndOfInput(Text input)
        => input.EndOfInput
            ? new Parsed<string>("", input)
            : new Error<string>(input, ErrorMessage.Expected("end of input"));
}
