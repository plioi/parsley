using System.Text.RegularExpressions;

namespace Parsley;

partial class Grammar
{
    public static Parser<string> Pattern(string name, string pattern, params RegexOptions[] regexOptions)
    {
        var regex = new TokenRegex(pattern, regexOptions);

        return input =>
        {
            var match = input.Match(regex);

            return match.Success
                ? new Parsed<string>(match.Value, input.Advance(match.Value.Length))
                : new Error<string>(input, ErrorMessage.Expected(name));
        };
    }
}

