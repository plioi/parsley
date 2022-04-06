using System.Globalization;

namespace Parsley.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ParserQueryTests
    {
        static readonly Parser<string> Next = new LambdaParser<string>(tokens => new Parsed<string>(tokens.Current.Literal, tokens.Advance()));

        static IEnumerable<Token> Tokenize(string input) => new CharLexer().Tokenize(input);

        public void CanBuildParserWhichSimulatesSuccessfulParsingOfGivenValueWithoutConsumingInput()
        {
            var parser = 1.SucceedWithThisValue();

            parser.PartiallyParses(Tokenize("input")).LeavingUnparsedTokens("i", "n", "p", "u", "t").WithValue(1);
        }

        public void CanBuildParserFromSingleSimplerParser()
        {
            var parser = from x in Next
                         select x.ToUpper(CultureInfo.InvariantCulture);

            parser.PartiallyParses(Tokenize("xy")).LeavingUnparsedTokens("y").WithValue("X");
        }

        public void CanBuildParserFromOrderedSequenceOfSimplerParsers()
        {
            var parser = (from a in Next
                          from b in Next
                          from c in Next
                          select (a + b + c).ToUpper(CultureInfo.InvariantCulture));

            parser.PartiallyParses(Tokenize("abcdef")).LeavingUnparsedTokens("d", "e", "f").WithValue("ABC");
        }

        public void PropogatesErrorsWithoutRunningRemainingParsers()
        {
            Parser<string> Fail = Grammar.Fail<string>();

            var tokens = Tokenize("xy").ToArray();

            (from _ in Fail
             from x in Next
             from y in Next
             select Tuple.Create(x, y)).FailsToParse(tokens).LeavingUnparsedTokens("x", "y");

            (from x in Next
             from _ in Fail
             from y in Next
             select Tuple.Create(x, y)).FailsToParse(tokens).LeavingUnparsedTokens("y");

            (from x in Next
             from y in Next
             from _ in Fail
             select Tuple.Create(x, y)).FailsToParse(tokens).AtEndOfInput();
        }
    }
}