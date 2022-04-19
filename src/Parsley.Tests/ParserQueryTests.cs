using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Parsley.Tests;

class ParserQueryTests
{
    static readonly Parser<char> Next = (ref Text input, ref Position position, [NotNullWhen(true)] out char value, [NotNullWhen(false)] out string? expectation) =>
    {
        var next = input.Peek(1);

        if (next.Length == 1)
        {
            char c = next[0];

            var positionDelta = input.Advance(position, 1);
            position.Move(positionDelta);

            expectation = null;
            value = c;
            return true;
        }

        expectation = "character";
        value = default;
        return false;
    };

    static readonly Parser<string> Fail = (ref Text input, ref Position position, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation) =>
    {
        expectation = "unsatisfiable expectation";
        value = null;
        return false;
    };

    public void CanBuildParserWhichSimulatesSuccessfulParsingOfGivenValueWithoutConsumingInput()
    {
        var parser = 1.SucceedWithThisValue();

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
