namespace Parsley.Tests
{
    using System;
    using System.Text;
    using Shouldly;
    using Xunit;

    public class TextTests
    {
        [Fact]
        public void CanPeekAheadNCharacters()
        {
            var empty = new Text("");
            empty.Peek(0).ShouldBe("");
            empty.Peek(1).ShouldBe("");

            var abc = new Text("abc");
            abc.Peek(0).ShouldBe("");
            abc.Peek(1).ShouldBe("a");
            abc.Peek(2).ShouldBe("ab");
            abc.Peek(3).ShouldBe("abc");
            abc.Peek(4).ShouldBe("abc");
            abc.Peek(100).ShouldBe("abc");
        }

        [Fact]
        public void CanAdvanceAheadNCharacters()
        {
            var empty = new Text("");
            empty.Advance(0).ToString().ShouldBe("");
            empty.Advance(1).ToString().ShouldBe("");

            var abc = new Text("abc");
            abc.Advance(0).ToString().ShouldBe("abc");
            abc.Advance(1).ToString().ShouldBe("bc");
            abc.Advance(2).ToString().ShouldBe("c");
            abc.Advance(3).ToString().ShouldBe("");
            abc.Advance(4).ToString().ShouldBe("");
            abc.Advance(100).ToString().ShouldBe("");
        }

        [Fact]
        public void DetectsTheEndOfInput()
        {
            new Text("!").EndOfInput.ShouldBeFalse();
            new Text("").EndOfInput.ShouldBeTrue();
        }

        [Fact]
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

        [Fact]
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

        [Fact]
        public void NormalizesLineEndingsToSingleLineFeedCharacter()
        {
            var multiline = new Text("Line 1\rLine 2\nLine 3\r\nLine 4");
            multiline.ToString().ShouldBe("Line 1\nLine 2\nLine 3\nLine 4");
        }

        [Fact]
        public void CanGetCurrentPosition()
        {
            var empty = new Text("");
            empty.Advance(0).Position.ShouldBe(new Position(1, 1));
            empty.Advance(1).Position.ShouldBe(new Position(1, 1));

            var lines = new StringBuilder()
                .AppendLine("Line 1")//Index 0-5, \n
                .AppendLine("Line 2")//Index 7-12, \n
                .AppendLine("Line 3");//Index 14-19, \n
            var list = new Text(lines.ToString());

            list.Advance(0).Position.ShouldBe(new Position(1, 1));
            list.Advance(5).Position.ShouldBe(new Position(1, 6));
            list.Advance(6).Position.ShouldBe(new Position(1, 7));

            list.Advance(7).Position.ShouldBe(new Position(2, 1));
            list.Advance(12).Position.ShouldBe(new Position(2, 6));
            list.Advance(13).Position.ShouldBe(new Position(2, 7));

            list.Advance(14).Position.ShouldBe(new Position(3, 1));
            list.Advance(19).Position.ShouldBe(new Position(3, 6));
            list.Advance(20).Position.ShouldBe(new Position(3, 7));

            list.Advance(21).Position.ShouldBe(new Position(4, 1));
            list.Advance(1000).Position.ShouldBe(new Position(4, 1));
        }
    }
}