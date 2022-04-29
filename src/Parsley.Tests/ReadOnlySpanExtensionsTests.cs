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
        var index = 0;

        empty.Advance(ref index, 0);
        index.ShouldBe(0);
        empty.ToString().ShouldBe("");

        empty.Advance(ref index, 1);
        index.ShouldBe(0);
        empty.ToString().ShouldBe("");

        ReadOnlySpan<char> abc = "abc";
        index = 0;

        abc.Advance(ref index, 0);
        index.ShouldBe(0);
        abc.ToString().ShouldBe("abc");

        var snapshot = abc;
        abc.Advance(ref index, 1);
        index.ShouldBe(1);
        abc.ToString().ShouldBe("bc");

        abc = snapshot;
        index = 0;
        abc.Advance(ref index, 2);
        index.ShouldBe(2);
        abc.ToString().ShouldBe("c");

        abc = snapshot;
        index = 0;
        abc.Advance(ref index, 3);
        index.ShouldBe(3);
        abc.ToString().ShouldBe("");

        abc = snapshot;
        index = 0;
        abc.Advance(ref index, 4);
        index.ShouldBe(3);
        abc.ToString().ShouldBe("");

        abc = snapshot;
        index = 0;
        abc.Advance(ref index, 100);
        index.ShouldBe(3);
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
        var index = 0;
        abc123.Advance(ref index, 2);
        index.ShouldBe(2);
        abc123.TakeWhile(digits).ToString().ShouldBe("");
        abc123.TakeWhile(letters).ToString().ShouldBe("c");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("c123");

        abc123 = snapshot;
        index = 0;
        abc123.Advance(ref index, 3);
        index.ShouldBe(3);
        abc123.TakeWhile(digits).ToString().ShouldBe("123");
        abc123.TakeWhile(letters).ToString().ShouldBe("");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("123");

        abc123 = snapshot;
        index = 0;
        abc123.Advance(ref index, 6);
        index.ShouldBe(6);
        abc123.TakeWhile(digits).ToString().ShouldBe("");
        abc123.TakeWhile(letters).ToString().ShouldBe("");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("");
    }

    public void CanTrackCurrentIndex()
    {
        ReadOnlySpan<char> empty = "";
        var index = 0;

        empty.Advance(ref index, 0);
        index.ShouldBe(0);

        empty.Advance(ref index, 1);
        index.ShouldBe(0);

        var lines = new StringBuilder()
            .Append("Line 1\n")//Index 0-5, \n
            .Append("Line 2\n")//Index 7-12, \n
            .Append("Line 3\n");//Index 14-19, \n
        ReadOnlySpan<char> list = lines.ToString();
        index = 0;

        var snapshot = list;
        list.Advance(ref index, 0);
        index.ShouldBe(0);

        list = snapshot;
        index = 0;
        list.Advance(ref index, 5);
        index.ShouldBe(5);

        list = snapshot;
        index = 0;
        list.Advance(ref index, 6);
        index.ShouldBe(6);


        list = snapshot;
        index = 0;
        list.Advance(ref index, 7);
        index.ShouldBe(7);

        list = snapshot;
        index = 0;
        list.Advance(ref index, 12);
        index.ShouldBe(12);

        list = snapshot;
        index = 0;
        list.Advance(ref index, 13);
        index.ShouldBe(13);


        list = snapshot;
        index = 0;
        list.Advance(ref index, 14);
        index.ShouldBe(14);

        list = snapshot;
        index = 0;
        list.Advance(ref index, 19);
        index.ShouldBe(19);

        list = snapshot;
        index = 0;
        list.Advance(ref index, 20);
        index.ShouldBe(20);


        list = snapshot;
        index = 0;
        list.Advance(ref index, 21);
        index.ShouldBe(21);

        list = snapshot;
        index = 0;
        list.Advance(ref index, 1000);
        index.ShouldBe(21);
    }
}
