using System;
using System.IO;

namespace Parsley
{
    public class LinedText : ILinedText
    {
        private string _lineBuffer;
        private readonly TextReader _reader;

        private int _line;
        private int _index;

        private bool _endOfInput;

        public LinedText(TextReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public string Peek(int characters)
        {
            if (_lineBuffer == null || characters <= 0)
                return string.Empty;

            var s = _index + characters >= _lineBuffer.Length
                       ? _lineBuffer.Substring(_index)
                       : _lineBuffer.Substring(_index, characters);

            return s;
        }

        public bool ReadLine()
        {
            _lineBuffer = _reader.ReadLine();

            if (_lineBuffer == null)
            {
                _endOfInput = true;
                return false;
            }

            _index = 0;
            ++_line;

            return true;
        }

        public void Advance(int characters)
        {
            if (characters <= 0)
                return;

            if (_lineBuffer == null)
                return;

            int index = Math.Min(_index + characters, _lineBuffer.Length);

            _index = index;
        }

        public bool EndOfInput => _endOfInput;
        public bool EndOfLine => _lineBuffer == null || _index >= _lineBuffer.Length;

        public MatchResult Match(TokenRegex regex)
        {
            if (_lineBuffer == null)
                return MatchResult.Fail;

            return regex.Match(_lineBuffer, _index);
        }

        public MatchResult Match(Predicate<char> test)
        {
            if (_lineBuffer == null)
                return MatchResult.Fail;

            int i = _index;

            while (i < _lineBuffer.Length && test(_lineBuffer[i]))
                i++;

            var value = Peek(i - _index);

            if (value.Length > 0)
                return MatchResult.Succeed(value);

            return MatchResult.Fail;
        }

        public Position Position => new Position(_line, _index + 1);

        public override string ToString()
        {
            if (_lineBuffer == null)
                return string.Empty;

            const int maxVisibleLength = 50;

            if (_lineBuffer.Length - _index >= maxVisibleLength)
                return _lineBuffer.Substring(_index, maxVisibleLength) + "...";

            return _lineBuffer.Substring(_index);
        }
    }
}