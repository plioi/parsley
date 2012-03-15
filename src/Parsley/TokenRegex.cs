using System.Text.RegularExpressions;

namespace Parsley
{
    public class TokenRegex
    {
        private readonly string pattern;
        private readonly Regex regex;

        public TokenRegex(string pattern)
        {
            this.pattern = pattern;
            regex = new Regex(@"\G(
                                       " + pattern + @"
                                       )", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
        }

        public MatchResult Match(string input, int index)
        {
            var match = regex.Match(input, index);

            if (match.Success)
                return MatchResult.Succeed(match.Value);

            return MatchResult.Fail();
        }

        public override string ToString()
        {
            return pattern;
        }
    }
}