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
            Predicate<char> letters = char.IsLetter;
            Predicate<char> digits = char.IsDigit;
            Predicate<char> alphanumerics = char.IsLetterOrDigit;

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
        public void CanGetCurrentPosition()
        {
            var empty = new Text("");
            empty.Advance(0).Position.ShouldBe(new Position(1, 1));
            empty.Advance(1).Position.ShouldBe(new Position(1, 1));

            var newLine = "\n";

            var lines = 
                  "Line 1" + newLine //Index 0-5,  \n
                + "Line 2" + newLine //Index 7-12, \n
                + "Line 3" + newLine;//Index 14-19,\n

            var list = new Text(lines, newLine);

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