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

        empty.Advance(0);
        empty.ToString().ShouldBe("");

        empty.Advance(1);
        empty.ToString().ShouldBe("");

        var abc = new Text("abc");

        abc.Advance(0);
        abc.ToString().ShouldBe("abc");

        var snapshot = abc.Snapshot();
        abc.Advance(1);
        abc.ToString().ShouldBe("bc");

        abc.Restore(snapshot);
        abc.Advance(2);
        abc.ToString().ShouldBe("c");

        abc.Restore(snapshot);
        abc.Advance(3);
        abc.ToString().ShouldBe("");

        abc.Restore(snapshot);
        abc.Advance(4);
        abc.ToString().ShouldBe("");

        abc.Restore(snapshot);
        abc.Advance(100);
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
        empty.TryMatch(letters, out var value).ShouldBe(false);
        value.ToString().ShouldBe("");

        var abc123 = new Text("abc123");
        abc123.TryMatch(digits, out value).ShouldBe(false);
        value.ToString().ShouldBe("");
        abc123.TryMatch(letters, out value).ShouldBe(true);
        value.ToString().ShouldBe("abc");
        abc123.TryMatch(alphanumerics, out value).ShouldBe(true);
        value.ToString().ShouldBe("abc123");

        abc123.Advance(2);
        abc123.TryMatch(digits, out value).ShouldBe(false);
        value.ToString().ShouldBe("");
        abc123.TryMatch(letters, out value).ShouldBe(true);
        value.ToString().ShouldBe("c");
        abc123.TryMatch(alphanumerics, out value).ShouldBe(true);
        value.ToString().ShouldBe("c123");

        abc123.Advance(1);
        abc123.TryMatch(digits, out value).ShouldBe(true);
        value.ToString().ShouldBe("123");
        abc123.TryMatch(letters, out value).ShouldBe(false);
        value.ToString().ShouldBe("");
        abc123.TryMatch(alphanumerics, out value).ShouldBe(true);
        value.ToString().ShouldBe("123");

        abc123.Advance(3);
        abc123.TryMatch(digits, out value).ShouldBe(false);
        value.ToString().ShouldBe("");
        abc123.TryMatch(letters, out value).ShouldBe(false);
        value.ToString().ShouldBe("");
        abc123.TryMatch(alphanumerics, out value).ShouldBe(false);
        value.ToString().ShouldBe("");
    }

    public void CanGetCurrentPosition()
    {
        var empty = new Text("");
        empty.Advance(0);empty.Position.ShouldBe(new Position(1, 1));
        empty.Advance(1);empty.Position.ShouldBe(new Position(1, 1));

        var lines = new StringBuilder()
            .Append("Line 1\n")//Index 0-5, \n
            .Append("Line 2\n")//Index 7-12, \n
            .Append("Line 3\n");//Index 14-19, \n
        var list = new Text(lines.ToString());

        var snapshot = list.Snapshot();
        list.Advance(0);
        list.Position.ShouldBe(new Position(1, 1));

        list.Restore(snapshot);
        list.Advance(5);
        list.Position.ShouldBe(new Position(1, 6));

        list.Restore(snapshot);
        list.Advance(6);
        list.Position.ShouldBe(new Position(1, 7));


        list.Restore(snapshot);
        list.Advance(7);
        list.Position.ShouldBe(new Position(2, 1));

        list.Restore(snapshot);
        list.Advance(12);
        list.Position.ShouldBe(new Position(2, 6));

        list.Restore(snapshot);
        list.Advance(13);
        list.Position.ShouldBe(new Position(2, 7));


        list.Restore(snapshot);
        list.Advance(14);
        list.Position.ShouldBe(new Position(3, 1));

        list.Restore(snapshot);
        list.Advance(19);
        list.Position.ShouldBe(new Position(3, 6));

        list.Restore(snapshot);
        list.Advance(20);
        list.Position.ShouldBe(new Position(3, 7));


        list.Restore(snapshot);
        list.Advance(21);
        list.Position.ShouldBe(new Position(4, 1));

        list.Restore(snapshot);
        list.Advance(1000);
        list.Position.ShouldBe(new Position(4, 1));
    }
}
