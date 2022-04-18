namespace Parsley;

partial class Grammar
{
    public static readonly Parser<string> EndOfInput =
        (ref Text input) => input.EndOfInput
            ? new Parsed<string>("")
            : new Error<string>("end of input");
}
