using System.Text;

namespace Parsley.Tests;

class TextTests
{
    public void CanPeekAheadNCharacters()
    {
        var empty = new Text("");
        empty.Peek(0).ToString().ShouldBe("");
        empty.Peek(1).ToString().ShouldBe("");

        var abc = new Text("abc");
        abc.Peek(0).ToString().ShouldBe("");
        abc.Peek(1).ToString().ShouldBe("a");
        abc.Peek(2).ToString().ShouldBe("ab");
        abc.Peek(3).ToString().ShouldBe("abc");
        abc.Peek(4).ToString().ShouldBe("abc");
        abc.Peek(100).ToString().ShouldBe("abc");
    }

    public void CanAdvanceAheadNCharactersWithSnapshotBacktracking()
    {
        var empty = new Text("");

        empty.Advance(0).ShouldBe((0, 0));
        empty.ToString().ShouldBe("");

        empty.Advance(1).ShouldBe((0, 0));
        empty.ToString().ShouldBe("");

        var abc = new Text("abc");

        abc.Advance(0).ShouldBe((0, 0));
        abc.ToString().ShouldBe("abc");

        var snapshot = abc;
        abc.Advance(1).ShouldBe((0, 1));
        abc.ToString().ShouldBe("bc");

        abc = snapshot;
        abc.Advance(2).ShouldBe((0, 2));
        abc.ToString().ShouldBe("c");

        abc = snapshot;
        abc.Advance(3).ShouldBe((0, 3));
        abc.ToString().ShouldBe("");

        abc = snapshot;
        abc.Advance(4).ShouldBe((0, 3));
        abc.ToString().ShouldBe("");

        abc = snapshot;
        abc.Advance(100).ShouldBe((0, 3));
        abc.ToString().ShouldBe("");
    }

    public void DetectsTheEndOfInput()
    {
        new Text("!").EndOfInput.ShouldBeFalse();
        new Text("").EndOfInput.ShouldBeTrue();
    }

    public void CanMatchLeadingCharactersByPredicate()
    {
        Predicate<char> letters = char.IsLetter;
        Predicate<char> digits = char.IsDigit;
        Predicate<char> alphanumerics = char.IsLetterOrDigit;

        var empty = new Text("");
        empty.TakeWhile(letters).ToString().ShouldBe("");

        var abc123 = new Text("abc123");
        abc123.TakeWhile(digits).ToString().ShouldBe("");
        abc123.TakeWhile(letters).ToString().ShouldBe("abc");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("abc123");

        abc123.Advance(2).ShouldBe((0, 2));
        abc123.TakeWhile(digits).ToString().ShouldBe("");
        abc123.TakeWhile(letters).ToString().ShouldBe("c");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("c123");

        abc123.Advance(1).ShouldBe((0, 1));
        abc123.TakeWhile(digits).ToString().ShouldBe("123");
        abc123.TakeWhile(letters).ToString().ShouldBe("");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("123");

        abc123.Advance(3).ShouldBe((0, 3));
        abc123.TakeWhile(digits).ToString().ShouldBe("");
        abc123.TakeWhile(letters).ToString().ShouldBe("");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("");
    }

    public void CanGetCurrentPosition()
    {
        var empty = new Text("");

        empty.Advance(0).ShouldBe((0, 0));
        empty.Position.ShouldBe(new Position(1, 1));

        empty.Advance(1).ShouldBe((0, 0));
        empty.Position.ShouldBe(new Position(1, 1));

        var lines = new StringBuilder()
            .Append("Line 1\n")//Index 0-5, \n
            .Append("Line 2\n")//Index 7-12, \n
            .Append("Line 3\n");//Index 14-19, \n
        var list = new Text(lines.ToString());

        var snapshot = list;
        list.Advance(0).ShouldBe((0, 0));
        list.Position.ShouldBe(new Position(1, 1));

        list = snapshot;
        list.Advance(5).ShouldBe((0, 5));
        list.Position.ShouldBe(new Position(1, 6));

        list = snapshot;
        list.Advance(6).ShouldBe((0, 6));
        list.Position.ShouldBe(new Position(1, 7));


        list = snapshot;
        list.Advance(7).ShouldBe((1, 0));
        list.Position.ShouldBe(new Position(2, 1));

        list = snapshot;
        list.Advance(12).ShouldBe((1, 5));
        list.Position.ShouldBe(new Position(2, 6));

        list = snapshot;
        list.Advance(13).ShouldBe((1, 6));
        list.Position.ShouldBe(new Position(2, 7));


        list = snapshot;
        list.Advance(14).ShouldBe((2, 0));
        list.Position.ShouldBe(new Position(3, 1));

        list = snapshot;
        list.Advance(19).ShouldBe((2, 5));
        list.Position.ShouldBe(new Position(3, 6));

        list = snapshot;
        list.Advance(20).ShouldBe((2, 6));
        list.Position.ShouldBe(new Position(3, 7));


        list = snapshot;
        list.Advance(21).ShouldBe((3, 0));
        list.Position.ShouldBe(new Position(4, 1));

        list = snapshot;
        list.Advance(1000).ShouldBe((3, 0));
        list.Position.ShouldBe(new Position(4, 1));
    }
}
