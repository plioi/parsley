using Parsimonious.Tests.Fixtures;
using Shouldly;
using System;
using System.IO;
using Xunit;

namespace Parsimonious.Tests
{
    public class LinedTextTests
    {
        [Fact]
        public void CanPeekAheadNCharacters()
        {
            using (var empty = new LinedTextTestFixture(""))
            {
                empty.Peek(0).ShouldBe("");
                empty.Peek(1).ShouldBe("");
                empty.ReadLine().ShouldBeFalse();
                empty.Peek(0).ShouldBe("");
                empty.Peek(1).ShouldBe("");
            }

            using (var t = new StringReader("abc"))
            {
                var abc = new LinedText(t);
                abc.ReadLine();
                abc.Peek(0).ShouldBe("");
                abc.Peek(1).ShouldBe("a");
                abc.Peek(2).ShouldBe("ab");
                abc.Peek(3).ShouldBe("abc");
                abc.Peek(4).ShouldBe("abc");
                abc.Peek(100).ShouldBe("abc");
            }
        }

        [Fact]
        public void CanAdvanceAheadNCharacters()
        {
            using (var empty = new LinedTextTestFixture(""))
            {
                empty.Advance(0);
                empty.ToString().ShouldBe("");
                empty.Advance(1);
                empty.ToString().ShouldBe("");
            }

            using (var abc = new LinedTextTestFixture("abc"))
            {
                abc.ReadLine().ShouldBeTrue();
                abc.ToString().ShouldBe("abc");
                abc.Advance(1);
                abc.ToString().ShouldBe("bc");
                abc.Advance(1);
                abc.ToString().ShouldBe("c");
                abc.Advance(1);
                abc.ToString().ShouldBe("");
                abc.Advance(1);
                abc.ToString().ShouldBe("");
                abc.Advance(100);
                abc.ToString().ShouldBe("");
            }
        }

        [Fact]
        public void DetectsTheEndOfInput()
        {
            using (var empty = new LinedTextTestFixture(""))
            {
                empty.EndOfInput.ShouldBeFalse();
                empty.EndOfLine.ShouldBeTrue();

                empty.ReadLine().ShouldBeFalse();

                empty.EndOfInput.ShouldBeTrue();
                empty.EndOfLine.ShouldBeTrue();

                empty.ReadLine().ShouldBeFalse();

                empty.EndOfInput.ShouldBeTrue();
                empty.EndOfLine.ShouldBeTrue();

                empty.ReadLine().ShouldBeFalse();

                empty.EndOfInput.ShouldBeTrue();
                empty.EndOfLine.ShouldBeTrue();
            }
            using (var x = new LinedTextTestFixture("x"))
            {
                x.EndOfInput.ShouldBeFalse();
                x.EndOfLine.ShouldBeTrue();

                x.ReadLine().ShouldBeTrue();

                x.EndOfInput.ShouldBeFalse();
                x.EndOfLine.ShouldBeFalse();

                x.ReadLine().ShouldBeFalse();

                x.EndOfInput.ShouldBeTrue();
                x.EndOfLine.ShouldBeTrue();
            }
            using (var asdf = new LinedTextTestFixture("asdf"))
            {
                asdf.EndOfInput.ShouldBeFalse();
                asdf.EndOfLine.ShouldBeTrue();

                asdf.ReadLine().ShouldBeTrue();

                asdf.EndOfInput.ShouldBeFalse();
                asdf.EndOfLine.ShouldBeFalse();

                asdf.Advance(1);

                asdf.EndOfInput.ShouldBeFalse();
                asdf.EndOfLine.ShouldBeFalse();

                asdf.Advance(2);

                asdf.EndOfInput.ShouldBeFalse();
                asdf.EndOfLine.ShouldBeFalse();

                asdf.Advance(1);

                asdf.EndOfInput.ShouldBeFalse();
                asdf.EndOfLine.ShouldBeTrue();

                asdf.ReadLine().ShouldBeFalse();

                asdf.EndOfInput.ShouldBeTrue();
                asdf.EndOfLine.ShouldBeTrue();
            }
        }

        [Fact]
        public void CanMatchLeadingCharactersByTokenRegex()
        {
            var end = new TokenRegex(@"$");
            var letters = new TokenRegex(@"[a-z]+");
            var digits = new TokenRegex(@"[0-9]+");
            var alphanumerics = new TokenRegex(@"[a-z0-9]+");

            using (var empty = new LinedTextTestFixture(""))
            {
                empty.ReadLine().ShouldBeFalse();
                empty.Match(letters).ShouldFail();
                empty.Match(end).ShouldFail(); // the behavior is different with respect to Text
            }

            using (var abc123 = new LinedTextTestFixture("abc123"))
            {
                abc123.ReadLine().ShouldBeTrue();

                abc123.Match(digits).ShouldFail();
                abc123.Match(letters).ShouldSucceed("abc");
                abc123.Match(alphanumerics).ShouldSucceed("abc123");

                abc123.Advance(2);

                abc123.Match(digits).ShouldFail();
                abc123.Match(letters).ShouldSucceed("c");
                abc123.Match(alphanumerics).ShouldSucceed("c123");

                abc123.Advance(1);
                
                abc123.Match(digits).ShouldSucceed("123");
                abc123.Match(letters).ShouldFail();
                abc123.Match(alphanumerics).ShouldSucceed("123");

                abc123.Advance(3);
                
                abc123.Match(digits).ShouldFail();
                abc123.Match(letters).ShouldFail();
                abc123.Match(alphanumerics).ShouldFail();
            }
        }

        [Fact]
        public void CanMatchLeadingCharactersByPredicate()
        {
            Predicate<char> letters = char.IsLetter;
            Predicate<char> digits = char.IsDigit;
            Predicate<char> alphanumerics = char.IsLetterOrDigit;

            using (var empty = new LinedTextTestFixture(""))
            {
                empty.ReadLine().ShouldBeFalse();
                empty.Match(letters).ShouldFail();
            }

            using (var abc123 = new LinedTextTestFixture("abc123"))
            {
                abc123.ReadLine().ShouldBeTrue();

                abc123.Match(digits).ShouldFail();
                abc123.Match(letters).ShouldSucceed("abc");
                abc123.Match(alphanumerics).ShouldSucceed("abc123");

                abc123.Advance(2);

                abc123.Match(digits).ShouldFail();
                abc123.Match(letters).ShouldSucceed("c");
                abc123.Match(alphanumerics).ShouldSucceed("c123");

                abc123.Advance(1);

                abc123.Match(digits).ShouldSucceed("123");
                abc123.Match(letters).ShouldFail();
                abc123.Match(alphanumerics).ShouldSucceed("123");

                abc123.Advance(3);
                
                abc123.Match(digits).ShouldFail();
                abc123.Match(letters).ShouldFail();
                abc123.Match(alphanumerics).ShouldFail();
            }
        }

        [Fact]
        public void CanGetCurrentPosition()
        {
            using (var empty = new LinedTextTestFixture(""))
            {
                empty.EndOfInput.ShouldBeFalse();
                empty.EndOfLine.ShouldBeTrue();
                empty.Position.ShouldBe(new Position(0, 1));

                empty.ReadLine().ShouldBeFalse();

                empty.EndOfInput.ShouldBeTrue();
                empty.EndOfLine.ShouldBeTrue();
                empty.Position.ShouldBe(new Position(0, 1));

                empty.Advance(0);

                empty.EndOfInput.ShouldBeTrue();
                empty.EndOfLine.ShouldBeTrue();
                empty.Position.ShouldBe(new Position(0, 1));

                empty.Advance(1);

                empty.EndOfInput.ShouldBeTrue();
                empty.EndOfLine.ShouldBeTrue();
                empty.Position.ShouldBe(new Position(0, 1));
            }

            var newLine = "\n";

            var lines =
                "Line 1" + newLine //Index 0-5, \n
                + "Line 2" + newLine //Index 7-12, \n
                + "Line 3" + newLine; //Index 14-19, \n

            using (var list = new LinedTextTestFixture(lines))
            {
                list.EndOfInput.ShouldBeFalse();
                list.EndOfLine.ShouldBeTrue();

                list.ReadLine().ShouldBeTrue();

                list.EndOfInput.ShouldBeFalse();
                list.EndOfLine.ShouldBeFalse();
                list.Position.ShouldBe(new Position(1, 1));

                list.Advance(0);

                list.EndOfInput.ShouldBeFalse();
                list.EndOfLine.ShouldBeFalse();
                list.Position.ShouldBe(new Position(1, 1));

                list.Advance(5);

                list.EndOfInput.ShouldBeFalse();
                list.EndOfLine.ShouldBeFalse();
                list.Position.ShouldBe(new Position(1, 6));

                list.Advance(1);

                list.EndOfInput.ShouldBeFalse();
                list.EndOfLine.ShouldBeTrue();
                list.Position.ShouldBe(new Position(1, 7));

                list.Advance(1);

                list.EndOfInput.ShouldBeFalse();
                list.EndOfLine.ShouldBeTrue();
                list.Position.ShouldBe(new Position(1, 7));

                list.ReadLine().ShouldBeTrue();

                list.EndOfInput.ShouldBeFalse();
                list.EndOfLine.ShouldBeFalse();
                list.Position.ShouldBe(new Position(2, 1));

                list.Advance(5);

                list.EndOfInput.ShouldBeFalse();
                list.EndOfLine.ShouldBeFalse();
                list.Position.ShouldBe(new Position(2, 6));

                list.Advance(1);

                list.EndOfInput.ShouldBeFalse();
                list.EndOfLine.ShouldBeTrue();
                list.Position.ShouldBe(new Position(2, 7));

                list.Advance(1);

                list.EndOfInput.ShouldBeFalse();
                list.EndOfLine.ShouldBeTrue();
                list.Position.ShouldBe(new Position(2, 7));
                
                list.ReadLine().ShouldBeTrue();

                list.EndOfInput.ShouldBeFalse();
                list.EndOfLine.ShouldBeFalse();
                list.Position.ShouldBe(new Position(3, 1));

                list.Advance(5);

                list.EndOfInput.ShouldBeFalse();
                list.EndOfLine.ShouldBeFalse();
                list.Position.ShouldBe(new Position(3, 6));

                list.Advance(1);

                list.Position.ShouldBe(new Position(3, 7));
                list.EndOfLine.ShouldBeTrue();

                list.Advance(100000);

                list.EndOfInput.ShouldBeFalse();
                list.EndOfLine.ShouldBeTrue();
                list.Position.ShouldBe(new Position(3, 7));

                list.ReadLine().ShouldBeFalse();

                list.EndOfInput.ShouldBeTrue();
                list.EndOfLine.ShouldBeTrue();
            }
        }

        [Fact]
        public void TextToStringShowsEllipsisForLongInputs()
        {
            const string complex =
                @"{""numbers"" : [10, 20, 30], ""window"": { ""title"": ""Sample Widget"", ""parent"": null, ""maximized"": true, ""transparent"": false}}";

            using (var text = new LinedTextTestFixture(complex))
            {
                text.ReadLine();
                text.ToString().ShouldBe(@"{""numbers"" : [10, 20, 30], ""window"": { ""title"": ""S...");
            }
        }
    }
}