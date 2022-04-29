namespace Parsley.Tests;

class ReadOnlySpanExtensionsTests
{
    public void CanPeekAheadNItems()
    {
        ReadOnlySpan<char> empty = "";
        empty.Peek(0, 0).ToString().ShouldBe("");
        empty.Peek(0, 1).ToString().ShouldBe("");

        ReadOnlySpan<char> abc = "abc";

        abc.Peek(0, 0).ToString().ShouldBe("");
        abc.Peek(0, 1).ToString().ShouldBe("a");
        abc.Peek(0, 2).ToString().ShouldBe("ab");
        abc.Peek(0, 3).ToString().ShouldBe("abc");
        abc.Peek(0, 4).ToString().ShouldBe("abc");
        abc.Peek(0, 100).ToString().ShouldBe("abc");

        abc.Peek(1, 0).ToString().ShouldBe("");
        abc.Peek(1, 1).ToString().ShouldBe("b");
        abc.Peek(1, 2).ToString().ShouldBe("bc");
        abc.Peek(1, 3).ToString().ShouldBe("bc");
        abc.Peek(1, 100).ToString().ShouldBe("bc");

        abc.Peek(2, 0).ToString().ShouldBe("");
        abc.Peek(2, 1).ToString().ShouldBe("c");
        abc.Peek(2, 2).ToString().ShouldBe("c");
        abc.Peek(2, 100).ToString().ShouldBe("c");

        abc.Peek(3, 0).ToString().ShouldBe("");
        abc.Peek(3, 1).ToString().ShouldBe("");
        abc.Peek(3, 2).ToString().ShouldBe("");
        abc.Peek(3, 100).ToString().ShouldBe("");
    }

    public void CanMatchLeadingItemsByPredicate()
    {
        Predicate<char> letters = char.IsLetter;
        Predicate<char> digits = char.IsDigit;
        Predicate<char> alphanumerics = char.IsLetterOrDigit;

        ReadOnlySpan<char> empty = "";
        empty.TakeWhile(0, letters).ToString().ShouldBe("");

        ReadOnlySpan<char> abc123 = "abc123";

        abc123.TakeWhile(0, digits).ToString().ShouldBe("");
        abc123.TakeWhile(0, letters).ToString().ShouldBe("abc");
        abc123.TakeWhile(0, alphanumerics).ToString().ShouldBe("abc123");

        abc123.TakeWhile(2, digits).ToString().ShouldBe("");
        abc123.TakeWhile(2, letters).ToString().ShouldBe("c");
        abc123.TakeWhile(2, alphanumerics).ToString().ShouldBe("c123");

        abc123.TakeWhile(3, digits).ToString().ShouldBe("123");
        abc123.TakeWhile(3, letters).ToString().ShouldBe("");
        abc123.TakeWhile(3, alphanumerics).ToString().ShouldBe("123");

        abc123.TakeWhile(6, digits).ToString().ShouldBe("");
        abc123.TakeWhile(6, letters).ToString().ShouldBe("");
        abc123.TakeWhile(6, alphanumerics).ToString().ShouldBe("");
    }
}
