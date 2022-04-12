namespace Parsley;

partial class Grammar
{
    public static readonly Parser<string> EndOfInput =
        input => input.EndOfInput
            ? new Parsed<string>("", input)
            : new Error<string>(input, ErrorMessage.Expected("end of input"));
}
