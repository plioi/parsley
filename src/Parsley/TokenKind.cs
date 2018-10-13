namespace Parsley
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    public abstract class TokenKind
    {
        public static readonly TokenKind EndOfInput = new Empty("end of input");
        public static readonly TokenKind Unknown = new Pattern("Unknown", @".+");

        protected TokenKind(string name, bool skippable = false)
        {
            this.Name = name;
            Skippable = skippable;
        }

        public bool TryMatch(Text text, out Token token)
        {
            var match = Match(text);

            if (match.Success)
            {
                token = new Token(this, text.Position, match.Value);
                return true;
            }

            token = null;
            return false;
        }

        protected abstract MatchResult Match(Text text);

        public string Name { get; }

        public bool Skippable { get; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Pattern : TokenKind
    {
        private readonly TokenRegex _regex;

        public Pattern(string name, string pattern, params RegexOptions[] regexOptions)
            : this(name, pattern, false, regexOptions)
        {
        }

        public Pattern(string name, string pattern, bool skippable, params RegexOptions[] regexOptions)
            : base(name, skippable)
        {
            _regex = new TokenRegex(pattern, regexOptions);
        }

        protected override MatchResult Match(Text text)
        {
            return text.Match(_regex);
        }
    }

    public class Keyword : Pattern
    {
        public Keyword(string keyword)
            : base(keyword, keyword + @"\b")
        {
            if (keyword.Any(ch => !char.IsLetter(ch)))
                throw new ArgumentException("Keywords may only contain letters.", nameof(keyword));
        }
    }

    public class Operator : TokenKind
    {
        private readonly string _symbol;

        public Operator(string symbol)
            : base(symbol)
        {
            _symbol = symbol;
        }

        protected override MatchResult Match(Text text)
        {
            var peek = text.Peek(_symbol.Length);

            if (peek == _symbol)
                return MatchResult.Succeed(peek);

            return MatchResult.Fail;
        }
    }

    public class Empty : TokenKind
    {
        public Empty(string name)
            : base(name) { }

        protected override MatchResult Match(Text text)
        {
            return MatchResult.Fail;
        }
    }
}