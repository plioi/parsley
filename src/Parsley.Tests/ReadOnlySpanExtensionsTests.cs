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

    public void CanCountLeadingItemsSatisfyingPredicate()
    {
        Func<char, bool> letters = char.IsLetter;
        Func<char, bool> digits = char.IsDigit;
        Func<char, bool> alphanumerics = char.IsLetterOrDigit;

        ReadOnlySpan<char> empty = "";
        empty.CountWhile(0, letters).ShouldBe(0);

        ReadOnlySpan<char> abc123 = "abc123";

        abc123.CountWhile(0, digits).ShouldBe(0);
        abc123.CountWhile(0, letters).ShouldBe(3);
        abc123.CountWhile(0, alphanumerics).ShouldBe(6);

        abc123.CountWhile(2, digits).ShouldBe(0);
        abc123.CountWhile(2, letters).ShouldBe(1);
        abc123.CountWhile(2, alphanumerics).ShouldBe(4);

        abc123.CountWhile(3, digits).ShouldBe(3);
        abc123.CountWhile(3, letters).ShouldBe(0);
        abc123.CountWhile(3, alphanumerics).ShouldBe(3);

        abc123.CountWhile(6, digits).ShouldBe(0);
        abc123.CountWhile(6, letters).ShouldBe(0);
        abc123.CountWhile(6, alphanumerics).ShouldBe(0);

        ReadOnlySpan<int> numbers = new[] { 2, 4, 6, 8, 1, 3, 5, 7, 9 };

        numbers.CountWhile(0, x => x % 2 == 0).ShouldBe(4);
        numbers.CountWhile(1, x => x % 2 == 0).ShouldBe(3);
        numbers.CountWhile(2, x => x % 2 == 0).ShouldBe(2);
        numbers.CountWhile(3, x => x % 2 == 0).ShouldBe(1);
        numbers.CountWhile(4, x => x % 2 == 0).ShouldBe(0);
        numbers.CountWhile(9, x => x % 2 == 0).ShouldBe(0);
    }
}
