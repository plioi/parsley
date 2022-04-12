namespace Parsley;

partial class Grammar
{
    public static readonly Parser<string> EndOfInput =
        input => input.EndOfInput
            ? new Parsed<string>("", input.Position, input.EndOfInput)
            : new Error<string>(input.Position, input.EndOfInput, ErrorMessage.Expected("end of input"));
}
