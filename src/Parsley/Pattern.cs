using System.Text.RegularExpressions;

namespace Parsley
{
    public class Pattern
    {
        private readonly string pattern;
        private readonly Regex regex;

        public Pattern(string pattern)
        {
            this.pattern = pattern;
            regex = new Regex(@"\G(
                                       " + pattern + @"
                                       )", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
        }

        public Match Match(string input, int index)
        {
            return regex.Match(input, index);
        }

        public override string ToString()
        {
            return pattern;
        }
    }
}