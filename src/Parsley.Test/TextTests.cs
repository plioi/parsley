using System.Text;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class TextTests
    {
        [Test]
        public void CanPeekAheadNCharacters()
        {
            var empty = new Text("");
            empty.Peek(0).ShouldEqual("");
            empty.Peek(1).ShouldEqual("");

            var abc = new Text("abc");
            abc.Peek(0).ShouldEqual("");
            abc.Peek(1).ShouldEqual("a");
            abc.Peek(2).ShouldEqual("ab");
            abc.Peek(3).ShouldEqual("abc");
            abc.Peek(4).ShouldEqual("abc");
            abc.Peek(100).ShouldEqual("abc");
        }

        [Test]
        public void CanAdvanceAheadNCharacters()
        {
            var empty = new Text("");
            empty.Advance(0).ToString().ShouldEqual("");
            empty.Advance(1).ToString().ShouldEqual("");

            var abc = new Text("abc");
            abc.Advance(0).ToString().ShouldEqual("abc");
            abc.Advance(1).ToString().ShouldEqual("bc");
            abc.Advance(2).ToString().ShouldEqual("c");
            abc.Advance(3).ToString().ShouldEqual("");
            abc.Advance(4).ToString().ShouldEqual("");
            abc.Advance(100).ToString().ShouldEqual("");
        }

        [Test]
        public void DetectsTheEndOfInput()
        {
            new Text("!").EndOfInput.ShouldBeFalse();
            new Text("").EndOfInput.ShouldBeTrue();
        }

        [Test]
        public void CanMatchLeadingCharactersByPattern()
        {
            var letters = new Pattern(@"[a-z]+");
            var digits = new Pattern(@"[0-9]+");
            var alphanumerics = new Pattern(@"[a-z0-9]+");

            var empty = new Text("");
            empty.Match(letters).Success.ShouldBeFalse();

            var abc123 = new Text("abc123");
            abc123.Match(digits).Success.ShouldBeFalse();
            abc123.Match(letters).Value.ShouldEqual("abc");
            abc123.Match(alphanumerics).Value.ShouldEqual("abc123");

            abc123.Advance(2).Match(digits).Success.ShouldBeFalse();
            abc123.Advance(2).Match(letters).Value.ShouldEqual("c");
            abc123.Advance(2).Match(alphanumerics).Value.ShouldEqual("c123");

            abc123.Advance(3).Match(digits).Value.ShouldEqual("123");
            abc123.Advance(3).Match(letters).Success.ShouldBeFalse();
            abc123.Advance(3).Match(alphanumerics).Value.ShouldEqual("123");

            abc123.Advance(6).Match(digits).Success.ShouldBeFalse();
            abc123.Advance(6).Match(letters).Success.ShouldBeFalse();
            abc123.Advance(6).Match(alphanumerics).Success.ShouldBeFalse();
        }

        [Test]
        public void CanGetCurrentPosition()
        {
            var empty = new Text("");
            empty.Advance(0).Position.ShouldEqual(new Position(1, 1));
            empty.Advance(1).Position.ShouldEqual(new Position(1, 1));

            var lines = new StringBuilder()
                .AppendLine("Line 1")//Index 0-5, \r\n
                .AppendLine("Line 2")//Index 8-13, \r\n
                .AppendLine("Line 3");//Index 16-21, \r\n
            var list = new Text(lines.ToString());

            list.Advance(0).Position.ShouldEqual(new Position(1, 1));
            list.Advance(5).Position.ShouldEqual(new Position(1, 6));
            list.Advance(7).Position.ShouldEqual(new Position(1, 8));

            list.Advance(8).Position.ShouldEqual(new Position(2, 1));
            list.Advance(13).Position.ShouldEqual(new Position(2, 6));
            list.Advance(15).Position.ShouldEqual(new Position(2, 8));

            list.Advance(16).Position.ShouldEqual(new Position(3, 1));
            list.Advance(21).Position.ShouldEqual(new Position(3, 6));
            list.Advance(23).Position.ShouldEqual(new Position(3, 8));

            list.Advance(24).Position.ShouldEqual(new Position(4, 1));
            list.Advance(1000).Position.ShouldEqual(new Position(4, 1));
        }
    }
}