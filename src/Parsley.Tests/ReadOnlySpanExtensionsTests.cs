using static Parsley.Characters;

using Shouldly;

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

        ReadOnlySpan<int> digits = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

        digits.Peek(0, 0).ToArray().ShouldBe([]);
        digits.Peek(0, 1).ToArray().ShouldBe([0]);
        digits.Peek(0, 2).ToArray().ShouldBe([0, 1]);
        digits.Peek(0, 3).ToArray().ShouldBe([0, 1, 2]);
        digits.Peek(0, 4).ToArray().ShouldBe([0, 1, 2, 3]);
        digits.Peek(0, 9).ToArray().ShouldBe([0, 1, 2, 3, 4, 5, 6, 7, 8]);
        digits.Peek(0, 10).ToArray().ShouldBe([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);
        digits.Peek(0, 100).ToArray().ShouldBe([0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);

        digits.Peek(6, 0).ToArray().ShouldBe([]);
        digits.Peek(6, 1).ToArray().ShouldBe([6]);
        digits.Peek(6, 2).ToArray().ShouldBe([6, 7]);
        digits.Peek(6, 3).ToArray().ShouldBe([6, 7, 8]);
        digits.Peek(6, 4).ToArray().ShouldBe([6, 7, 8, 9]);
        digits.Peek(6, 100).ToArray().ShouldBe([6, 7, 8, 9]);

        digits.Peek(10, 0).ToArray().ShouldBe([]);
        digits.Peek(10, 1).ToArray().ShouldBe([]);
        digits.Peek(10, 2).ToArray().ShouldBe([]);
        digits.Peek(10, 3).ToArray().ShouldBe([]);
        digits.Peek(10, 4).ToArray().ShouldBe([]);
        digits.Peek(10, 100).ToArray().ShouldBe([]);
    }

    public void CanCountLeadingItemsSatisfyingPredicate()
    {
        ReadOnlySpan<char> empty = "";
        empty.CountWhile(0, IsLetter).ShouldBe(0);

        ReadOnlySpan<char> abc123 = "abc123";

        abc123.CountWhile(0, IsDigit).ShouldBe(0);
        abc123.CountWhile(0, IsLetter).ShouldBe(3);
        abc123.CountWhile(0, IsLetterOrDigit).ShouldBe(6);

        abc123.CountWhile(2, IsDigit).ShouldBe(0);
        abc123.CountWhile(2, IsLetter).ShouldBe(1);
        abc123.CountWhile(2, IsLetterOrDigit).ShouldBe(4);

        abc123.CountWhile(3, IsDigit).ShouldBe(3);
        abc123.CountWhile(3, IsLetter).ShouldBe(0);
        abc123.CountWhile(3, IsLetterOrDigit).ShouldBe(3);

        abc123.CountWhile(6, IsDigit).ShouldBe(0);
        abc123.CountWhile(6, IsLetter).ShouldBe(0);
        abc123.CountWhile(6, IsLetterOrDigit).ShouldBe(0);

        ReadOnlySpan<int> numbers = [2, 4, 6, 8, 1, 3, 5, 7, 9];

        numbers.CountWhile(0, x => x % 2 == 0).ShouldBe(4);
        numbers.CountWhile(1, x => x % 2 == 0).ShouldBe(3);
        numbers.CountWhile(2, x => x % 2 == 0).ShouldBe(2);
        numbers.CountWhile(3, x => x % 2 == 0).ShouldBe(1);
        numbers.CountWhile(4, x => x % 2 == 0).ShouldBe(0);
        numbers.CountWhile(9, x => x % 2 == 0).ShouldBe(0);
    }

    public void CanCountLeadingItemsSatisfyingPredicateUpToSomeMaxCount()
    {
        ReadOnlySpan<char> empty = "";
        empty.CountWhile(0, IsLetter, 0).ShouldBe(0);
        empty.CountWhile(0, IsLetter, 10).ShouldBe(0);

        ReadOnlySpan<char> abc123 = "abc123";

        abc123.CountWhile(0, IsDigit, maxCount: 0).ShouldBe(0);
        abc123.CountWhile(0, IsDigit, maxCount: 3).ShouldBe(0);
        abc123.CountWhile(0, IsDigit, maxCount: 4).ShouldBe(0);
        abc123.CountWhile(0, IsDigit, maxCount: 10).ShouldBe(0);

        abc123.CountWhile(0, IsLetter, maxCount: 0).ShouldBe(0);
        abc123.CountWhile(0, IsLetter, maxCount: 3).ShouldBe(3);
        abc123.CountWhile(0, IsLetter, maxCount: 4).ShouldBe(3);
        abc123.CountWhile(0, IsLetter, maxCount: 10).ShouldBe(3);

        abc123.CountWhile(0, IsLetterOrDigit, maxCount: 0).ShouldBe(0);
        abc123.CountWhile(0, IsLetterOrDigit, maxCount: 3).ShouldBe(3);
        abc123.CountWhile(0, IsLetterOrDigit, maxCount: 4).ShouldBe(4);
        abc123.CountWhile(0, IsLetterOrDigit, maxCount: 10).ShouldBe(6);

        abc123.CountWhile(2, IsDigit, maxCount: 0).ShouldBe(0);
        abc123.CountWhile(2, IsDigit, maxCount: 3).ShouldBe(0);
        abc123.CountWhile(2, IsDigit, maxCount: 4).ShouldBe(0);
        abc123.CountWhile(2, IsDigit, maxCount: 10).ShouldBe(0);

        abc123.CountWhile(2, IsLetter, maxCount: 0).ShouldBe(0);
        abc123.CountWhile(2, IsLetter, maxCount: 3).ShouldBe(1);
        abc123.CountWhile(2, IsLetter, maxCount: 4).ShouldBe(1);
        abc123.CountWhile(2, IsLetter, maxCount: 10).ShouldBe(1);

        abc123.CountWhile(2, IsLetterOrDigit, maxCount: 0).ShouldBe(0);
        abc123.CountWhile(2, IsLetterOrDigit, maxCount: 3).ShouldBe(3);
        abc123.CountWhile(2, IsLetterOrDigit, maxCount: 4).ShouldBe(4);
        abc123.CountWhile(2, IsLetterOrDigit, maxCount: 10).ShouldBe(4);

        abc123.CountWhile(3, IsDigit, maxCount: 0).ShouldBe(0);
        abc123.CountWhile(3, IsDigit, maxCount: 3).ShouldBe(3);
        abc123.CountWhile(3, IsDigit, maxCount: 4).ShouldBe(3);
        abc123.CountWhile(3, IsDigit, maxCount: 10).ShouldBe(3);

        abc123.CountWhile(3, IsLetter, maxCount: 0).ShouldBe(0);
        abc123.CountWhile(3, IsLetter, maxCount: 3).ShouldBe(0);
        abc123.CountWhile(3, IsLetter, maxCount: 4).ShouldBe(0);
        abc123.CountWhile(3, IsLetter, maxCount: 10).ShouldBe(0);

        abc123.CountWhile(3, IsLetterOrDigit, maxCount: 0).ShouldBe(0);
        abc123.CountWhile(3, IsLetterOrDigit, maxCount: 3).ShouldBe(3);
        abc123.CountWhile(3, IsLetterOrDigit, maxCount: 4).ShouldBe(3);
        abc123.CountWhile(3, IsLetterOrDigit, maxCount: 10).ShouldBe(3);

        abc123.CountWhile(6, IsDigit, maxCount: 0).ShouldBe(0);
        abc123.CountWhile(6, IsDigit, maxCount: 3).ShouldBe(0);
        abc123.CountWhile(6, IsDigit, maxCount: 4).ShouldBe(0);
        abc123.CountWhile(6, IsDigit, maxCount: 10).ShouldBe(0);

        abc123.CountWhile(6, IsLetter, maxCount: 0).ShouldBe(0);
        abc123.CountWhile(6, IsLetter, maxCount: 3).ShouldBe(0);
        abc123.CountWhile(6, IsLetter, maxCount: 4).ShouldBe(0);
        abc123.CountWhile(6, IsLetter, maxCount: 10).ShouldBe(0);

        abc123.CountWhile(6, IsLetterOrDigit, maxCount: 0).ShouldBe(0);
        abc123.CountWhile(6, IsLetterOrDigit, maxCount: 3).ShouldBe(0);
        abc123.CountWhile(6, IsLetterOrDigit, maxCount: 4).ShouldBe(0);
        abc123.CountWhile(6, IsLetterOrDigit, maxCount: 10).ShouldBe(0);

        ReadOnlySpan<int> numbers = [2, 4, 6, 8, 1, 3, 5, 7, 9];

        numbers.CountWhile(0, x => x % 2 == 0, maxCount: 0).ShouldBe(0);
        numbers.CountWhile(1, x => x % 2 == 0, maxCount: 0).ShouldBe(0);
        numbers.CountWhile(2, x => x % 2 == 0, maxCount: 0).ShouldBe(0);
        numbers.CountWhile(3, x => x % 2 == 0, maxCount: 0).ShouldBe(0);
        numbers.CountWhile(4, x => x % 2 == 0, maxCount: 0).ShouldBe(0);
        numbers.CountWhile(9, x => x % 2 == 0, maxCount: 0).ShouldBe(0);

        numbers.CountWhile(0, x => x % 2 == 0, maxCount: 3).ShouldBe(3);
        numbers.CountWhile(1, x => x % 2 == 0, maxCount: 3).ShouldBe(3);
        numbers.CountWhile(2, x => x % 2 == 0, maxCount: 3).ShouldBe(2);
        numbers.CountWhile(3, x => x % 2 == 0, maxCount: 3).ShouldBe(1);
        numbers.CountWhile(4, x => x % 2 == 0, maxCount: 3).ShouldBe(0);
        numbers.CountWhile(9, x => x % 2 == 0, maxCount: 3).ShouldBe(0);

        numbers.CountWhile(0, x => x % 2 == 0, maxCount: 4).ShouldBe(4);
        numbers.CountWhile(1, x => x % 2 == 0, maxCount: 4).ShouldBe(3);
        numbers.CountWhile(2, x => x % 2 == 0, maxCount: 4).ShouldBe(2);
        numbers.CountWhile(3, x => x % 2 == 0, maxCount: 4).ShouldBe(1);
        numbers.CountWhile(4, x => x % 2 == 0, maxCount: 4).ShouldBe(0);
        numbers.CountWhile(9, x => x % 2 == 0, maxCount: 4).ShouldBe(0);

        numbers.CountWhile(0, x => x % 2 == 0, maxCount: 10).ShouldBe(4);
        numbers.CountWhile(1, x => x % 2 == 0, maxCount: 10).ShouldBe(3);
        numbers.CountWhile(2, x => x % 2 == 0, maxCount: 10).ShouldBe(2);
        numbers.CountWhile(3, x => x % 2 == 0, maxCount: 10).ShouldBe(1);
        numbers.CountWhile(4, x => x % 2 == 0, maxCount: 10).ShouldBe(0);
        numbers.CountWhile(9, x => x % 2 == 0, maxCount: 10).ShouldBe(0);
    }
}
