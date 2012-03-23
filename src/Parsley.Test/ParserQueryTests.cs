using System;
using Xunit;

namespace Parsley
{
    public class ParserQueryTests
    {
        private static readonly Parser<string> Next = new LambdaParser<string>(tokens => new Parsed<string>(tokens.Current.Literal, tokens.Advance()));

        private static TokenStream Tokenize(string source)
        {
            return new CharTokenStream(source);
        }

        [Fact]
        public void CanBuildParserWhichSimulatesSuccessfulParsingOfGivenValueWithoutConsumingInput()
        {
            var parser = 1.SucceedWithThisValue();

            parser.PartiallyParses(Tokenize("input")).LeavingUnparsedTokens("i", "n", "p", "u", "t").IntoValue(1);
        }

        [Fact]
        public void CanBuildParserFromSingleSimplerParser()
        {
            var parser = from x in Next
                         select x.ToUpper();

            parser.PartiallyParses(Tokenize("xy")).LeavingUnparsedTokens("y").IntoValue("X");
        }

        [Fact]
        public void CanBuildParserFromOrderedSequenceOfSimplerParsers()
        {
            var parser = (from a in Next
                          from b in Next
                          from c in Next
                          select (a + b + c).ToUpper());

            parser.PartiallyParses(Tokenize("abcdef")).LeavingUnparsedTokens("d", "e", "f").IntoValue("ABC");
        }

        [Fact]
        public void PropogatesErrorsWithoutRunningRemainingParsers()
        {
            Parser<string> Fail = Grammar.Fail<string>();

            var source = Tokenize("xy");

            (from _ in Fail
             from x in Next
             from y in Next
             select Tuple.Create(x, y)).FailsToParse(source).LeavingUnparsedTokens("x", "y");

            (from x in Next
             from _ in Fail
             from y in Next
             select Tuple.Create(x, y)).FailsToParse(source).LeavingUnparsedTokens("y");

            (from x in Next
             from y in Next
             from _ in Fail
             select Tuple.Create(x, y)).FailsToParse(source).AtEndOfInput();
        }
    }
}