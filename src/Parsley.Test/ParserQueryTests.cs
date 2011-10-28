using System;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class ParserQueryTests
    {
        private static readonly Parser<string> Next = new LambdaParser<string>(tokens => new Parsed<string>(tokens.CurrentToken.Literal, tokens.Advance()));

        private static Lexer Tokenize(string source)
        {
            return new CharLexer(source);
        }

        [Test]
        public void CanBuildParserWhichSimulatesSuccessfulParsingOfGivenValueWithoutConsumingInput()
        {
            var parser = 1.SucceedWithThisValue();

            parser.PartiallyParses(Tokenize("input"), "input").IntoValue(1);
        }

        [Test]
        public void CanBuildParserFromSingleSimplerParser()
        {
            var parser = from x in Next
                         select x.ToUpper();

            parser.PartiallyParses(Tokenize("xy"), "y").IntoValue("X");
        }

        [Test]
        public void CanBuildParserFromOrderedSequenceOfSimplerParsers()
        {
            var parser = (from a in Next
                          from b in Next
                          from c in Next
                          select (a + b + c).ToUpper());

            parser.PartiallyParses(Tokenize("abcdef"), "def").IntoValue("ABC");
        }

        [Test]
        public void PropogatesErrorsWithoutRunningRemainingParsers()
        {
            Parser<string> Fail = Grammar.Fail<string>();

            var source = Tokenize("xy");

            (from _ in Fail
             from x in Next
             from y in Next
             select Tuple.Create(x, y)).FailsToParse(source, "xy");

            (from x in Next
             from _ in Fail
             from y in Next
             select Tuple.Create(x, y)).FailsToParse(source, "y");

            (from x in Next
             from y in Next
             from _ in Fail
             select Tuple.Create(x, y)).FailsToParse(source, "");
        }
    }
}