using System;
using System.Text;
using Should;

namespace Parsley
{
    public class TextTests
    {
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

        public void DetectsTheEndOfInput()
        {
            new Text("!").EndOfInput.ShouldBeFalse();
            new Text("").EndOfInput.ShouldBeTrue();
        }

        public void CanMatchLeadingCharactersByTokenRegex()
        {
            var end = new TokenRegex(@"$");
            var letters = new TokenRegex(@"[a-z]+");
            var digits = new TokenRegex(@"[0-9]+");
            var alphanumerics = new TokenRegex(@"[a-z0-9]+");

            var empty = new Text("");
            empty.Match(letters).ShouldFail();
            empty.Match(end).ShouldSucceed("");

            var abc123 = new Text("abc123");
            abc123.Match(digits).ShouldFail();
            abc123.Match(letters).ShouldSucceed("abc");
            abc123.Match(alphanumerics).ShouldSucceed("abc123");

            abc123.Advance(2).Match(digits).ShouldFail();
            abc123.Advance(2).Match(letters).ShouldSucceed("c");
            abc123.Advance(2).Match(alphanumerics).ShouldSucceed("c123");

            abc123.Advance(3).Match(digits).ShouldSucceed("123");
            abc123.Advance(3).Match(letters).ShouldFail();
            abc123.Advance(3).Match(alphanumerics).ShouldSucceed("123");

            abc123.Advance(6).Match(digits).ShouldFail();
            abc123.Advance(6).Match(letters).ShouldFail();
            abc123.Advance(6).Match(alphanumerics).ShouldFail();
        }

        public void CanMatchLeadingCharactersByPredicate()
        {
            Predicate<char> letters = Char.IsLetter;
            Predicate<char> digits = Char.IsDigit;
            Predicate<char> alphanumerics = Char.IsLetterOrDigit;

            var empty = new Text("");
            empty.Match(letters).ShouldFail();

            var abc123 = new Text("abc123");
            abc123.Match(digits).ShouldFail();
            abc123.Match(letters).ShouldSucceed("abc");
            abc123.Match(alphanumerics).ShouldSucceed("abc123");

            abc123.Advance(2).Match(digits).ShouldFail();
            abc123.Advance(2).Match(letters).ShouldSucceed("c");
            abc123.Advance(2).Match(alphanumerics).ShouldSucceed("c123");

            abc123.Advance(3).Match(digits).ShouldSucceed("123");
            abc123.Advance(3).Match(letters).ShouldFail();
            abc123.Advance(3).Match(alphanumerics).ShouldSucceed("123");

            abc123.Advance(6).Match(digits).ShouldFail();
            abc123.Advance(6).Match(letters).ShouldFail();
            abc123.Advance(6).Match(alphanumerics).ShouldFail();
        }

        public void NormalizesLineEndingsToSingleLineFeedCharacter()
        {
            var multiline = new Text("Line 1\rLine 2\nLine 3\r\nLine 4");
            multiline.ToString().ShouldEqual("Line 1\nLine 2\nLine 3\nLine 4");
        }

        public void CanGetCurrentPosition()
        {
            var empty = new Text("");
            empty.Advance(0).Position.ShouldEqual(new Position(1, 1));
            empty.Advance(1).Position.ShouldEqual(new Position(1, 1));

            var lines = new StringBuilder()
                .AppendLine("Line 1")//Index 0-5, \n
                .AppendLine("Line 2")//Index 7-12, \n
                .AppendLine("Line 3");//Index 14-19, \n
            var list = new Text(lines.ToString());

            list.Advance(0).Position.ShouldEqual(new Position(1, 1));
            list.Advance(5).Position.ShouldEqual(new Position(1, 6));
            list.Advance(6).Position.ShouldEqual(new Position(1, 7));

            list.Advance(7).Position.ShouldEqual(new Position(2, 1));
            list.Advance(12).Position.ShouldEqual(new Position(2, 6));
            list.Advance(13).Position.ShouldEqual(new Position(2, 7));

            list.Advance(14).Position.ShouldEqual(new Position(3, 1));
            list.Advance(19).Position.ShouldEqual(new Position(3, 6));
            list.Advance(20).Position.ShouldEqual(new Position(3, 7));

            list.Advance(21).Position.ShouldEqual(new Position(4, 1));
            list.Advance(1000).Position.ShouldEqual(new Position(4, 1));
        }
    }
}