using System.Linq;
using System.Text.RegularExpressions;

namespace Parsley
{
    public class Text
    {
        private readonly int index;
        private readonly string source;
        private readonly int line;

        public Text(string source)
            : this(NormalizeLineEndings(source), 0, 1) { }

        private Text(string source, int index, int line)
        {
            this.source = source;
            this.index = index;

            if (index > source.Length)
                this.index = source.Length;

            this.line = line;
        }

        public string Peek(int characters)
        {
            return index + characters >= source.Length
                       ? source.Substring(index)
                       : source.Substring(index, characters);
        }

        public Text Advance(int characters)
        {
            if (characters == 0)
                return this;

            int newIndex = index + characters;
            int newLineNumber = line + Peek(characters).Count(ch => ch == '\n');
            
            return new Text(source, newIndex, newLineNumber);
        }

        public bool EndOfInput
        {
            get { return index >= source.Length; }
        }

        public Match Match(Pattern pattern)
        {
            return pattern.Match(source, index);
        }

        private int Column
        {
            get
            {
                if (index == 0)
                    return 1;

                int indexOfPreviousNewLine = source.LastIndexOf('\n', index - 1);
                return index - indexOfPreviousNewLine;
            }
        }

        public Position Position
        {
            get { return new Position(line, Column); }
        }

        public override string ToString()
        {
            return source.Substring(index);
        }

        private static string NormalizeLineEndings(string source)
        {
            return source.Replace("\r\n", "\n").Replace('\r', '\n');
        }
    }
}