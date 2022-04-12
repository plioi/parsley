using System.Globalization;

namespace Parsley.Tests;

class ParserQueryTests
{
    static readonly Parser<string> Next = input =>
    {
        var next = input.Peek(1);
        input.Advance(1);

        return new Parsed<string>(next, input.Position, input.EndOfInput);
    };

    public void CanBuildParserWhichSimulatesSuccessfulParsingOfGivenValueWithoutConsumingInput()
    {
        var parser = 1.SucceedWithThisValue();

        parser.PartiallyParses("input", "input").WithValue(1);
    }

    public void CanBuildParserFromSingleSimplerParser()
    {
        var parser = from x in Next
            select x.ToUpper(CultureInfo.InvariantCulture);

        parser.PartiallyParses("xy", "y").WithValue("X");
    }

    public void CanBuildParserFromOrderedSequenceOfSimplerParsers()
    {
        var parser = (from a in Next
            from b in Next
            from c in Next
            select (a + b + c).ToUpper(CultureInfo.InvariantCulture));

        parser.PartiallyParses("abcdef", "def").WithValue("ABC");
    }

    public void PropogatesErrorsWithoutRunningRemainingParsers()
    {
        var Fail = Grammar<string>.Fail;

        (from _ in Fail
            from x in Next
            from y in Next
            select Tuple.Create(x, y)).FailsToParse("xy", "xy");

        (from x in Next
            from _ in Fail
            from y in Next
            select Tuple.Create(x, y)).FailsToParse("xy", "y");

        (from x in Next
            from y in Next
            from _ in Fail
            select Tuple.Create(x, y)).FailsToParse("xy");
    }
}
