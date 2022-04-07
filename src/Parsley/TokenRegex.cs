using System.Text.RegularExpressions;

namespace Parsley;

public class TokenRegex
{
    readonly string pattern;
    readonly Regex regex;

    public TokenRegex(string pattern, params RegexOptions[] regexOptions)
    {
        var options = RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace;

        foreach (var additionalOption in regexOptions)
            options |= additionalOption;

        this.pattern = pattern;
        regex = new Regex(@"\G(
                                       " + pattern + @"
                                       )", options);
    }

    public MatchResult Match(string input, int index)
    {
        var match = regex.Match(input, index);

        if (match.Success)
            return MatchResult.Succeed(match.Value);

        return MatchResult.Fail;
    }

    public override string ToString()
    {
        return pattern;
    }
}
