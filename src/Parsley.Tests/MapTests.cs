using System.Globalization;
using static Parsley.Grammar;

using Fixie.Assertions;

namespace Parsley.Tests;

class MapTests
{
    static readonly Parser<char, char> Next = Single<char>(_ => true, "character");

    static readonly Parser<char, string> Fail = (ReadOnlySpan<char> input, ref int index, out bool succeeded, out string? expectation) =>
    {
        expectation = "unsatisfiable expectation";
        succeeded = false;
        return null;
    };

    public void CanMapOneParser()
    {
        Map(Next, x => char.ToUpper(x, CultureInfo.InvariantCulture))
            .PartiallyParses("xy", "y").ShouldBe('X');

        Map(Fail, x => x)
            .FailsToParse("xy", "xy", "unsatisfiable expectation expected");
    }

    public void CanMapTwoParsers()
    {
        Map(Next, Next,
                (a, b) => $"{a}{b}".ToUpper(CultureInfo.InvariantCulture))
            .PartiallyParses("abcdef", "cdef").ShouldBe("AB");

        Map(Fail, Next,
                (_, x) => x)
            .FailsToParse("xy", "xy", "unsatisfiable expectation expected");

        Map(Next, Fail,
                (x, _) => x)
            .FailsToParse("xy", "y", "unsatisfiable expectation expected");
    }

    public void CanMapThreeParsers()
    {
        Map(Next, Next, Next,
                (a, b, c) => $"{a}{b}{c}".ToUpper(CultureInfo.InvariantCulture))
            .PartiallyParses("abcdef", "def").ShouldBe("ABC");

        Map(Fail, Next, Next,
                (_, x, y) => (x, y))
            .FailsToParse("xy", "xy", "unsatisfiable expectation expected");

        Map(Next, Fail, Next,
            (x, _, y) => (x, y))
                .FailsToParse("xy", "y", "unsatisfiable expectation expected");

        Map(Next, Next, Fail,
                (x, y, _) => (x, y))
            .FailsToParse("xy", "", "unsatisfiable expectation expected");
    }

    public void CanMapFourParsers()
    {
        Map(Next, Next, Next, Next,
                (a, b, c, d) => $"{a}{b}{c}{d}".ToUpper(CultureInfo.InvariantCulture))
            .PartiallyParses("abcdef", "ef").ShouldBe("ABCD");

        Map(Fail, Next, Next, Next,
                (_, x, y, z) => (x, y, z))
            .FailsToParse("xyz", "xyz", "unsatisfiable expectation expected");

        Map(Next, Fail, Next, Next,
                (x, _, y, z) => (x, y, z))
            .FailsToParse("xyz", "yz", "unsatisfiable expectation expected");

        Map(Next, Next, Fail, Next,
                (x, y, _, z) => (x, y, z))
            .FailsToParse("xyz", "z", "unsatisfiable expectation expected");

        Map(Next, Next, Next, Fail,
                (x, y, z, _) => (x, y, z))
            .FailsToParse("xyz", "", "unsatisfiable expectation expected");
    }

    public void CanMapFiveParsers()
    {
        Map(Next, Next, Next, Next, Next,
                (a, b, c, d, e) => $"{a}{b}{c}{d}{e}".ToUpper(CultureInfo.InvariantCulture))
            .PartiallyParses("abcdef", "f").ShouldBe("ABCDE");

        Map(Fail, Next, Next, Next, Next,
                (_, w, x, y, z) => (w, x, y, z))
            .FailsToParse("wxyz", "wxyz", "unsatisfiable expectation expected");

        Map(Next, Fail, Next, Next, Next,
                (w, _, x, y, z) => (w, x, y, z))
            .FailsToParse("wxyz", "xyz", "unsatisfiable expectation expected");

        Map(Next, Next, Fail, Next, Next,
                (w, x, _, y, z) => (w, x, y, z))
            .FailsToParse("wxyz", "yz", "unsatisfiable expectation expected");

        Map(Next, Next, Next, Fail, Next,
                (w, x, y, _, z) => (w, x, y, z))
            .FailsToParse("wxyz", "z", "unsatisfiable expectation expected");

        Map(Next, Next, Next, Next, Fail,
                (w, x, y, z, _) => (w, x, y, z))
            .FailsToParse("wxyz", "", "unsatisfiable expectation expected");
    }
}
