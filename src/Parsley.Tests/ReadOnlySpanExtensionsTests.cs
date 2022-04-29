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
        abc123 = abc123.Slice(2);
        index += 2;
        index.ShouldBe(2);
        abc123.TakeWhile(digits).ToString().ShouldBe("");
        abc123.TakeWhile(letters).ToString().ShouldBe("c");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("c123");

        abc123 = snapshot;
        index = 0;
        abc123 = abc123.Slice(3);
        index += 3;
        index.ShouldBe(3);
        abc123.TakeWhile(digits).ToString().ShouldBe("123");
        abc123.TakeWhile(letters).ToString().ShouldBe("");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("123");

        abc123 = snapshot;
        index = 0;
        abc123 = abc123.Slice(6);
        index += 6;
        index.ShouldBe(6);
        abc123.TakeWhile(digits).ToString().ShouldBe("");
        abc123.TakeWhile(letters).ToString().ShouldBe("");
        abc123.TakeWhile(alphanumerics).ToString().ShouldBe("");
    }
}
