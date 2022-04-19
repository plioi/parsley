using System.Text;

namespace Parsley.Tests;

class TextExtensionsTests
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
        var position = new Position(1, 1);

        empty.Advance(ref position, 0);
        position.ShouldBe(new(1, 1));
        empty.ToString().ShouldBe("");

        empty.Advance(ref position, 1);
        position.ShouldBe(new(1, 1));
        empty.ToString().ShouldBe("");

        var abc = new Text("abc");
        position = new Position(1, 1);

        abc.Advance(ref position, 0);
        position.ShouldBe(new(1, 1));
        abc.ToString().ShouldBe("abc");

        var snapshot = abc;
        abc.Advance(ref position, 1);
        position.ShouldBe(new(1, 2));
        abc.ToString().ShouldBe("bc");

        abc = snapshot;
        position = new Position(1, 1);
        abc.Advance(ref position, 2);
        position.ShouldBe(new(1, 3));
        abc.ToString().ShouldBe("c");

        abc = snapshot;
        position = new Position(1, 1);
        abc.Advance(ref position, 3);
        position.ShouldBe(new(1, 4));
        abc.ToString().ShouldBe("");

        abc = snapshot;
        position = new Position(1, 1);
        abc.Advance(ref position, 4);
        position.ShouldBe(new(1, 4));
        abc.ToString().ShouldBe("");

        abc = snapshot;
        position = new Position(1, 1);
        abc.Advance(ref position, 100);
        position.ShouldBe(new(1, 4));
        abc.ToString().ShouldBe("");
    }

    public void DetectsTheEndOfInput()
    {
        var nonempty = new Text("!");
        nonempty.EndOfInput().ShouldBeFalse();

        var empty = new Text("");
        empty.EndOfInput().ShouldBeTrue();
    }

    public void CanMatchLeadingCharactersByPredicate()
    {
        Predicate<char> letters = char.IsLetter;
        Predicate<char> digits = char.IsDigit;
        Predicate<char> alphanumerics = char.IsLetterOrDigit;

        var empty = new Text("");
        empty.TakeWhile(letters).ToString().ShouldBe("");

        var abc123 = new Text("abc123");
        var snapshot = abc123;

        abc123.TakeWhile(digits).ToString().ShouldBe("");
        abc123.TakeWhile(letters).ToString().ShouldBe("abc");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("abc123");

        abc123 = snapshot;
        var position = new Position(1, 1);
        abc123.Advance(ref position, 2);
        position.ShouldBe(new(1, 3));
        abc123.TakeWhile(digits).ToString().ShouldBe("");
        abc123.TakeWhile(letters).ToString().ShouldBe("c");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("c123");

        abc123 = snapshot;
        position = new Position(1, 1);
        abc123.Advance(ref position, 3);
        position.ShouldBe(new(1, 4));
        abc123.TakeWhile(digits).ToString().ShouldBe("123");
        abc123.TakeWhile(letters).ToString().ShouldBe("");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("123");

        abc123 = snapshot;
        position = new Position(1, 1);
        abc123.Advance(ref position, 6);
        position.ShouldBe(new(1, 7));
        abc123.TakeWhile(digits).ToString().ShouldBe("");
        abc123.TakeWhile(letters).ToString().ShouldBe("");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("");
    }

    public void CanTrackCurrentPosition()
    {
        var empty = new Text("");
        var position = new Position(1, 1);

        empty.Advance(ref position, 0);
        position.ShouldBe(new Position(1, 1));

        empty.Advance(ref position, 1);
        position.ShouldBe(new Position(1, 1));

        var lines = new StringBuilder()
            .Append("Line 1\n")//Index 0-5, \n
            .Append("Line 2\n")//Index 7-12, \n
            .Append("Line 3\n");//Index 14-19, \n
        var list = new Text(lines.ToString());
        position = new Position(1, 1);

        var snapshot = list;
        list.Advance(ref position, 0);
        position.ShouldBe(new Position(1, 1));

        list = snapshot;
        position = new(1, 1);
        list.Advance(ref position, 5);
        position.ShouldBe(new Position(1, 6));

        list = snapshot;
        position = new(1, 1);
        list.Advance(ref position, 6);
        position.ShouldBe(new Position(1, 7));


        list = snapshot;
        position = new(1, 1);
        list.Advance(ref position, 7);
        position.ShouldBe(new Position(2, 1));

        list = snapshot;
        position = new(1, 1);
        list.Advance(ref position, 12);
        position.ShouldBe(new Position(2, 6));

        list = snapshot;
        position = new(1, 1);
        list.Advance(ref position, 13);
        position.ShouldBe(new Position(2, 7));


        list = snapshot;
        position = new(1, 1);
        list.Advance(ref position, 14);
        position.ShouldBe(new Position(3, 1));

        list = snapshot;
        position = new(1, 1);
        list.Advance(ref position, 19);
        position.ShouldBe(new Position(3, 6));

        list = snapshot;
        position = new(1, 1);
        list.Advance(ref position, 20);
        position.ShouldBe(new Position(3, 7));


        list = snapshot;
        position = new(1, 1);
        list.Advance(ref position, 21);
        position.ShouldBe(new Position(4, 1));

        list = snapshot;
        position = new(1, 1);
        list.Advance(ref position, 1000);
        position.ShouldBe(new Position(4, 1));
    }
}
