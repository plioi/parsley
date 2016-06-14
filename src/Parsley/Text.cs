namespace Parsley
{
    using System;
    using System.Linq;

    public class Text
    {
        private readonly int index;
        private readonly string input;
        private readonly int line;

        public Text(string input)
            : this(NormalizeLineEndings(input), 0, 1) { }

        private Text(string input, int index, int line)
        {
            this.input = input;
            this.index = index;

            if (index > input.Length)
                this.index = input.Length;

            this.line = line;
        }

        public string Peek(int characters)
        {
            return index + characters >= input.Length
                       ? input.Substring(index)
                       : input.Substring(index, characters);
        }

        public Text Advance(int characters)
        {
            if (characters == 0)
                return this;

            int newIndex = index + characters;
            int newLineNumber = line + Peek(characters).Cast<char>().Count(ch => ch == '\n');
            
            return new Text(input, newIndex, newLineNumber);
        }

        public bool EndOfInput
        {
            get { return index >= input.Length; }
        }

        public MatchResult Match(TokenRegex regex)
        {
            return regex.Match(input, index);
        }

        public MatchResult Match(Predicate<char> test)
        {
            int i = index;

            while (i < input.Length && test(input[i]))
                i++;

            var value = Peek(i - index);

            if (value.Length > 0)
                return MatchResult.Succeed(value);

            return MatchResult.Fail;
        }

        private int Column
        {
            get
            {
                if (index == 0)
                    return 1;

                int indexOfPreviousNewLine = input.LastIndexOf('\n', index - 1);
                return index - indexOfPreviousNewLine;
            }
        }

        public Position Position
        {
            get { return new Position(line, Column); }
        }

        public override string ToString()
        {
            return input.Substring(index);
        }

        private static string NormalizeLineEndings(string input)
        {
            return input.Replace("\r\n", "\n").Replace('\r', '\n');
        }
    }
}