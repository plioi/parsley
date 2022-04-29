using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using static Parsley.Grammar;

namespace Parsley.Tests;

class ParserQueryTests
{
    static readonly Parser<char, char> Next = Single(_ => true, "character");

    static readonly Parser<char, string> Fail = (ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation) =>
    {
        expectation = "unsatisfiable expectation";
        value = null;
        return false;
    };

    public void CanBuildParserWhichSimulatesSuccessfulParsingOfGivenValueWithoutConsumingInput()
    {
        var parser = 1.SucceedWithThisValue<int>();

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
            select Tuple.Create(x, y)).FailsToParse("xy", "xy", "unsatisfiable expectation expected");

        (from x in Next
            from _ in Fail
            from y in Next
            select Tuple.Create(x, y)).FailsToParse("xy", "y", "unsatisfiable expectation expected");

        (from x in Next
            from y in Next
            from _ in Fail
            select Tuple.Create(x, y)).FailsToParse("xy", "", "unsatisfiable expectation expected");
    }
}
