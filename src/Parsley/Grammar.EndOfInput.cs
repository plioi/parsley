namespace Parsley;

partial class Grammar
{
    public static readonly Parser<string> EndOfInput =
        input => input.EndOfInput
            ? new Parsed<string>("", input.Position)
            : new Error<string>(input.Position, ErrorMessage.Expected("end of input"));
}
