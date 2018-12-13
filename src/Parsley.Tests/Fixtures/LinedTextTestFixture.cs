using System;
using System.IO;

namespace Parsley.Tests.Fixtures
{
    class LinedTextTestFixture : IDisposable, ILinedText
    {
        public LinedTextTestFixture(string text)
        {
            _reader = new StringReader(text);
            _text = new LinedText(_reader);
        }

        private readonly StringReader _reader;
        private readonly LinedText _text;

        public bool EndOfInput => _text.EndOfInput;

        public bool EndOfLine => _text.EndOfLine;

        public Position Position => _text.Position;

        public void Dispose()
        {
            _reader.Dispose();
        }

        public string Peek(int characters)
        {
            return _text.Peek(characters);
        }

        public void Advance(int characters)
        {
            _text.Advance(characters);
        }

        public MatchResult Match(TokenRegex regex)
        {
            return _text.Match(regex);
        }

        public MatchResult Match(Predicate<char> test)
        {
            return _text.Match(test);
        }

        public bool ReadLine()
        {
            return _text.ReadLine();
        }

        public override string ToString()
        {
            return _text.ToString();
        }
    }
}
