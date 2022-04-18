using System.Diagnostics.CodeAnalysis;

namespace Parsley;

partial class Grammar
{
    public static readonly Parser<string> EndOfInput =
        (ref Text input, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation) =>
        {
            if (input.EndOfInput)
            {
                expectation = null;
                value = "";
                return true;
            }

            expectation = "end of input";
            value = null;
            return false;
        };
}
