using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Parsley
{
    public class TokenKind
    {
        private readonly string name;
        private readonly Pattern pattern;
        private readonly bool skippable;

        public TokenKind(string name, string pattern, bool skippable = false)
        {
            this.name = name;
            this.pattern = new Pattern(pattern);
            this.skippable = skippable;
        }

        public bool TryMatch(Text text, out Token token)
        {
            var match = text.Match(pattern);

            if (match.Success)
            {
                token = new Token(this, text.Position, match.Value);
                return true;
            }

            token = null;
            return false;
        }

        public string Name
        {
            get { return name; }
        }

        public bool Skippable
        {
            get { return skippable;}
        }
    }

    public class Keyword : TokenKind
    {
        public Keyword(string word) : base(word, word + @"\b")
        {
            if (word.Any(ch => !Char.IsLetter(ch)))
                throw new ArgumentException("Keywords may only contain letters.", "word");
        }
    }

    public class Operator : TokenKind
    {
        public Operator(string symbol) : base(symbol, Regex.Escape(symbol)) { }
    }
}