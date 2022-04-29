using System.Text;

namespace Parsley.Tests;

class ReadOnlySpanExtensionsTests
{
    public void CanPeekAheadNItems()
    {
        ReadOnlySpan<char> empty = "";
        empty.Peek(0).ToString().ShouldBe("");
        empty.Peek(1).ToString().ShouldBe("");

        ReadOnlySpan<char> abc = "abc";
        abc.Peek(0).ToString().ShouldBe("");
        abc.Peek(1).ToString().ShouldBe("a");
        abc.Peek(2).ToString().ShouldBe("ab");
        abc.Peek(3).ToString().ShouldBe("abc");
        abc.Peek(4).ToString().ShouldBe("abc");
        abc.Peek(100).ToString().ShouldBe("abc");
    }

    public void CanAdvanceAheadNItemsWithSnapshotBacktracking()
    {
        ReadOnlySpan<char> empty = "";
        var position = 0;

        empty.Advance(ref position, 0);
        position.ShouldBe(0);
        empty.ToString().ShouldBe("");

        empty.Advance(ref position, 1);
        position.ShouldBe(0);
        empty.ToString().ShouldBe("");

        ReadOnlySpan<char> abc = "abc";
        position = 0;

        abc.Advance(ref position, 0);
        position.ShouldBe(0);
        abc.ToString().ShouldBe("abc");

        var snapshot = abc;
        abc.Advance(ref position, 1);
        position.ShouldBe(1);
        abc.ToString().ShouldBe("bc");

        abc = snapshot;
        position = 0;
        abc.Advance(ref position, 2);
        position.ShouldBe(2);
        abc.ToString().ShouldBe("c");

        abc = snapshot;
        position = 0;
        abc.Advance(ref position, 3);
        position.ShouldBe(3);
        abc.ToString().ShouldBe("");

        abc = snapshot;
        position = 0;
        abc.Advance(ref position, 4);
        position.ShouldBe(3);
        abc.ToString().ShouldBe("");

        abc = snapshot;
        position = 0;
        abc.Advance(ref position, 100);
        position.ShouldBe(3);
        abc.ToString().ShouldBe("");
    }

    public void CanMatchLeadingItemsByPredicate()
    {
        Predicate<char> letters = char.IsLetter;
        Predicate<char> digits = char.IsDigit;
        Predicate<char> alphanumerics = char.IsLetterOrDigit;

        ReadOnlySpan<char> empty = "";
        empty.TakeWhile(letters).ToString().ShouldBe("");

        ReadOnlySpan<char> abc123 = "abc123";
        var snapshot = abc123;

        abc123.TakeWhile(digits).ToString().ShouldBe("");
        abc123.TakeWhile(letters).ToString().ShouldBe("abc");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("abc123");

        abc123 = snapshot;
        var position = 0;
        abc123.Advance(ref position, 2);
        position.ShouldBe(2);
        abc123.TakeWhile(digits).ToString().ShouldBe("");
        abc123.TakeWhile(letters).ToString().ShouldBe("c");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("c123");

        abc123 = snapshot;
        position = 0;
        abc123.Advance(ref position, 3);
        position.ShouldBe(3);
        abc123.TakeWhile(digits).ToString().ShouldBe("123");
        abc123.TakeWhile(letters).ToString().ShouldBe("");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("123");

        abc123 = snapshot;
        position = 0;
        abc123.Advance(ref position, 6);
        position.ShouldBe(6);
        abc123.TakeWhile(digits).ToString().ShouldBe("");
        abc123.TakeWhile(letters).ToString().ShouldBe("");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("");
    }

    public void CanTrackCurrentPosition()
    {
        ReadOnlySpan<char> empty = "";
        var position = 0;

        empty.Advance(ref position, 0);
        position.ShouldBe(0);

        empty.Advance(ref position, 1);
        position.ShouldBe(0);

        var lines = new StringBuilder()
            .Append("Line 1\n")//Index 0-5, \n
            .Append("Line 2\n")//Index 7-12, \n
            .Append("Line 3\n");//Index 14-19, \n
        ReadOnlySpan<char> list = lines.ToString();
        position = 0;

        var snapshot = list;
        list.Advance(ref position, 0);
        position.ShouldBe(0);

        list = snapshot;
        position = 0;
        list.Advance(ref position, 5);
        position.ShouldBe(5);

        list = snapshot;
        position = 0;
        list.Advance(ref position, 6);
        position.ShouldBe(6);


        list = snapshot;
        position = 0;
        list.Advance(ref position, 7);
        position.ShouldBe(7);

        list = snapshot;
        position = 0;
        list.Advance(ref position, 12);
        position.ShouldBe(12);

        list = snapshot;
        position = 0;
        list.Advance(ref position, 13);
        position.ShouldBe(13);


        list = snapshot;
        position = 0;
        list.Advance(ref position, 14);
        position.ShouldBe(14);

        list = snapshot;
        position = 0;
        list.Advance(ref position, 19);
        position.ShouldBe(19);

        list = snapshot;
        position = 0;
        list.Advance(ref position, 20);
        position.ShouldBe(20);


        list = snapshot;
        position = 0;
        list.Advance(ref position, 21);
        position.ShouldBe(21);

        list = snapshot;
        position = 0;
        list.Advance(ref position, 1000);
        position.ShouldBe(21);
    }
}
