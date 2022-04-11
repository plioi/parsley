using System.Text.RegularExpressions;

namespace Parsley.Primitives;

class PatternParser : IParser<string>
{
    readonly string name;
    readonly TokenRegex regex;

    public PatternParser(string name, string pattern, params RegexOptions[] regexOptions)
    {
        this.name = name;
        regex = new TokenRegex(pattern, regexOptions);
    }

    public Reply<string> Parse(Text input)
    {
        var match = input.Match(regex);

        return match.Success
            ? new Parsed<string>(match.Value, input.Advance(match.Value.Length))
            : new Error<string>(input, ErrorMessage.Expected(name));
    }
}
