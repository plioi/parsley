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

        ReadOnlySpan<int> digits = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        digits.Peek(0, 0).ToArray().ShouldBe(Array.Empty<int>());
        digits.Peek(0, 1).ToArray().ShouldBe(new[] { 0 });
        digits.Peek(0, 2).ToArray().ShouldBe(new[] { 0, 1 });
        digits.Peek(0, 3).ToArray().ShouldBe(new[] { 0, 1, 2 });
        digits.Peek(0, 4).ToArray().ShouldBe(new[] { 0, 1, 2, 3 });
        digits.Peek(0, 9).ToArray().ShouldBe(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 });
        digits.Peek(0, 10).ToArray().ShouldBe(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        digits.Peek(0, 100).ToArray().ShouldBe(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

        digits.Peek(6, 0).ToArray().ShouldBe(Array.Empty<int>());
        digits.Peek(6, 1).ToArray().ShouldBe(new[] { 6 });
        digits.Peek(6, 2).ToArray().ShouldBe(new[] { 6, 7 });
        digits.Peek(6, 3).ToArray().ShouldBe(new[] { 6, 7, 8 });
        digits.Peek(6, 4).ToArray().ShouldBe(new[] { 6, 7, 8, 9 });
        digits.Peek(6, 100).ToArray().ShouldBe(new[] { 6, 7, 8, 9 });

        digits.Peek(10, 0).ToArray().ShouldBe(Array.Empty<int>());
        digits.Peek(10, 1).ToArray().ShouldBe(Array.Empty<int>());
        digits.Peek(10, 2).ToArray().ShouldBe(Array.Empty<int>());
        digits.Peek(10, 3).ToArray().ShouldBe(Array.Empty<int>());
        digits.Peek(10, 4).ToArray().ShouldBe(Array.Empty<int>());
        digits.Peek(10, 100).ToArray().ShouldBe(Array.Empty<int>());
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

        ReadOnlySpan<int> numbers = new[] { 2, 4, 6, 8, 1, 3, 5, 7, 9 };

        numbers.TakeWhile(0, x => x % 2 == 0).ToArray().ShouldBe(new[] { 2, 4, 6, 8 });
        numbers.TakeWhile(1, x => x % 2 == 0).ToArray().ShouldBe(new[] { 4, 6, 8 });
        numbers.TakeWhile(2, x => x % 2 == 0).ToArray().ShouldBe(new[] { 6, 8 });
        numbers.TakeWhile(3, x => x % 2 == 0).ToArray().ShouldBe(new[] { 8 });
        numbers.TakeWhile(4, x => x % 2 == 0).ToArray().ShouldBe(Array.Empty<int>());
        numbers.TakeWhile(9, x => x % 2 == 0).ToArray().ShouldBe(Array.Empty<int>());
    }
}
