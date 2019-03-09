namespace Parsimonious.Tests.Fixtures
{
    internal class TextTestFixture
    {
        public TextTestFixture(string s, string newLine = null)
        {
            _s = s;
            _newLine = newLine;
        }

        private readonly string _s;
        private readonly string _newLine;

        public Text Advance(int characters)
        {
            var text = new Text(_s, _newLine);

            text.Advance(characters);

            return text;
        }

        public static implicit operator Text(TextTestFixture f)
        {
            return new Text(f._s, f._newLine);
        }
    }
}
