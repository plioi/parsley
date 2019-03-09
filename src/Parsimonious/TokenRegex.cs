namespace Parsimonious
{
    using System.Text.RegularExpressions;

    public class TokenRegex
    {
        private readonly string _pattern;
        private readonly Regex _regex;

        public TokenRegex(string pattern, params RegexOptions[] regexOptions)
        {
            var options = RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace;

            foreach (var additionalOption in regexOptions)
                options |= additionalOption;

            _pattern = pattern;
            _regex = new Regex(@"\G(
                                       " + pattern + @"
                                       )", options);
        }

        public MatchResult Match(string input, int index)
        {
            var match = _regex.Match(input, index);

            if (match.Success)
                return MatchResult.Succeed(match.Value);

            return MatchResult.Fail;
        }

        public override string ToString()
        {
            return _pattern;
        }
    }
}