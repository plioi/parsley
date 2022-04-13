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

            if (match.Success)
            {
                input.Advance(match.Value.Length);

                return new Parsed<string>(match.Value, input.Position);
            }

            return new Error<string>(input.Position, ErrorMessage.Expected(name));
        };
    }
}

