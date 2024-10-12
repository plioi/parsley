using System.Globalization;
using static Parsley.Grammar;

using Shouldly;

namespace Parsley.Tests;

class ParserQueryTests
{
    static readonly Parser<char, char> Next = Single<char>(_ => true, "character");

    static readonly Parser<char, string> Fail = (ReadOnlySpan<char> input, ref int index, out bool succeeded, out string? expectation) =>
    {
        expectation = "unsatisfiable expectation";
        succeeded = false;
        return null;
    };

    public void CanBuildParserWhichSimulatesSuccessfulParsingOfGivenValueWithoutConsumingInput()
    {
        var parser = 1.SucceedWithThisValue<char, int>();

        parser.PartiallyParses("input", "input").ShouldBe(1);
    }

    public void CanBuildParserFromSingleSimplerParser()
    {
        var parser = from x in Next
            select char.ToUpper(x, CultureInfo.InvariantCulture);

        parser.PartiallyParses("xy", "y").ShouldBe('X');
    }

    public void CanBuildParserFromOrderedSequenceOfSimplerParsers()
    {
        var parser = (from a in Next
            from b in Next
            from c in Next
            select $"{a}{b}{c}".ToUpper(CultureInfo.InvariantCulture));

        parser.PartiallyParses("abcdef", "def").ShouldBe("ABC");
    }

    public void PropogatesErrorsWithoutRunningRemainingParsers()
    {
        (from _ in Fail
            from x in Next
            from y in Next
            select (x, y)).FailsToParse("xy", "xy", "unsatisfiable expectation expected");

        (from x in Next
            from _ in Fail
            from y in Next
            select (x, y)).FailsToParse("xy", "y", "unsatisfiable expectation expected");

        (from x in Next
            from y in Next
            from _ in Fail
            select (x, y)).FailsToParse("xy", "", "unsatisfiable expectation expected");
    }
}
