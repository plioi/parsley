using System;

namespace Parsley
{
    public struct Text
    {
        private readonly int _index;
        private readonly string _input;
        private readonly int _line;
        private readonly int _column;

        private readonly string _newLine;

        public Text(string input, string newLine = null)
            : this(input, 0, 1, 1, newLine ?? Environment.NewLine)
        { }

        private Text(string input, int index, int line, int column, string newLine)
        {
            _input = input;
            _index = index;

            if (index > input.Length)
                _index = input.Length;

            _line = line;
            _column = column;

            _newLine = newLine;
        }

        public string Peek(int characters)
        {
            var s = _index + characters >= _input.Length
                       ? _input.Substring(_index)
                       : _input.Substring(_index, characters);

            return s;
        }

        public Text Advance(int characters)
        {
            if (characters == 0)
                return this;

            int index = Math.Min(_index + characters, _input.Length);

            var newLineLength = _newLine.Length;

            var addedLinesCount = 0;
            var column = 1;

            var lastComparisonIndex = Math.Min(_index + characters, _input.Length) - newLineLength;

            for (var i = _index; i <= lastComparisonIndex; ++i)
            {
                var match = true;

                for (var j = 0; j < newLineLength; ++j)
                {
                    if (_input[i + j] != _newLine[j])
                    {
                        match = false;
                        break;
                    }
                }

                ++column;

                if (!match)
                    continue;

                ++addedLinesCount;
                column = 1;
            }

            if (addedLinesCount == 0)
                return new Text(_input, index, _line, _column + index - _index, _newLine);

            return new Text(_input, index, _line + addedLinesCount, column, _newLine);
        }

        public bool EndOfInput => _index >= _input.Length;

        public MatchResult Match(TokenRegex regex)
        {
            return regex.Match(_input, _index);
        }

        public MatchResult Match(Predicate<char> test)
        {
            int i = _index;

            while (i < _input.Length && test(_input[i]))
                i++;

            var value = Peek(i - _index);

            if (value.Length > 0)
                return MatchResult.Succeed(value);

            return MatchResult.Fail;
        }

        public Position Position => new Position(_line, _column);

        public override string ToString()
        {
            return _input.Substring(_index);
        }
    }
}