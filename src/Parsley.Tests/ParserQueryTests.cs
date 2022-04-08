using System.Globalization;
using static Parsley.Grammar;

namespace Parsley.Tests;

class ParserQueryTests
{
    static readonly IParser<string> Next = new LambdaParser<string>(input => new Parsed<string>(input.Peek(1), input.Advance(1)));

    public void CanBuildParserWhichSimulatesSuccessfulParsingOfGivenValueWithoutConsumingInput()
    {
        var parser = 1.SucceedWithThisValue();

        parser.PartiallyParses("input").LeavingUnparsedInput("input").WithValue(1);
    }

    public void CanBuildParserFromSingleSimplerParser()
    {
        var parser = from x in Next
            select x.ToUpper(CultureInfo.InvariantCulture);

        parser.PartiallyParses("xy").LeavingUnparsedInput("y").WithValue("X");
    }

    public void CanBuildParserFromOrderedSequenceOfSimplerParsers()
    {
        var parser = (from a in Next
            from b in Next
            from c in Next
            select (a + b + c).ToUpper(CultureInfo.InvariantCulture));

        parser.PartiallyParses("abcdef").LeavingUnparsedInput("def").WithValue("ABC");
    }

    public void PropogatesErrorsWithoutRunningRemainingParsers()
    {
        var Fail = Fail<string>();

        (from _ in Fail
            from x in Next
            from y in Next
            select Tuple.Create(x, y)).FailsToParse("xy").LeavingUnparsedInput("xy");

        (from x in Next
            from _ in Fail
            from y in Next
            select Tuple.Create(x, y)).FailsToParse("xy").LeavingUnparsedInput("y");

        (from x in Next
            from y in Next
            from _ in Fail
            select Tuple.Create(x, y)).FailsToParse("xy").AtEndOfInput();
    }
}
