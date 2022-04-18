using System.Globalization;

namespace Parsley.Tests;

class ParserQueryTests
{
    static readonly Parser<char> Next = (ref Text input) =>
    {
        var next = input.Peek(1);

        if (next.Length == 1)
        {
            char c = next[0];

            input.Advance(1);

            return new Parsed<char>(c);
        }

        return new Error<char>("character");
    };

    public void CanBuildParserWhichSimulatesSuccessfulParsingOfGivenValueWithoutConsumingInput()
    {
        var parser = 1.SucceedWithThisValue();

        parser.PartiallyParses("input", "input").Value.ShouldBe(1);
    }

    public void CanBuildParserFromSingleSimplerParser()
    {
        var parser = from x in Next
            select char.ToUpper(x, CultureInfo.InvariantCulture);

        parser.PartiallyParses("xy", "y").Value.ShouldBe('X');
    }

    public void CanBuildParserFromOrderedSequenceOfSimplerParsers()
    {
        var parser = (from a in Next
            from b in Next
            from c in Next
            select $"{a}{b}{c}".ToUpper(CultureInfo.InvariantCulture));

        parser.PartiallyParses("abcdef", "def").Value.ShouldBe("ABC");
    }

    public void PropogatesErrorsWithoutRunningRemainingParsers()
    {
        var Fail = Grammar<string>.Fail;

        (from _ in Fail
            from x in Next
            from y in Next
            select Tuple.Create(x, y)).FailsToParse("xy", "xy", "(1, 1): Parse error.");

        (from x in Next
            from _ in Fail
            from y in Next
            select Tuple.Create(x, y)).FailsToParse("xy", "y", "(1, 2): Parse error.");

        (from x in Next
            from y in Next
            from _ in Fail
            select Tuple.Create(x, y)).FailsToParse("xy", "", "(1, 3): Parse error.");
    }
}
