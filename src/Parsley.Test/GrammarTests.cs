using System;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class GrammarTests : Grammar
    {
        private static Lexer Tokenize(string source)
        {
            return new SampleLexer(source);
        }

        private class SampleLexer : Lexer
        {
            public static readonly TokenKind Digit = new TokenKind("Digit", @"[0-9]");
            public static readonly TokenKind Letter = new TokenKind("Letter", @"[a-zA-Z]");
            public static readonly TokenKind Symbol = new TokenKind("Symbol", @".");

            public SampleLexer(string source)
                : base(new Text(source), Digit, Letter, Symbol) { }
        }

        private Parser<Token> A, B, AB, COMMA;

        [SetUp]
        public void SetUp()
        {
            A = Token("A");
            B = Token("B");

            AB = from a in A
                 from b in B
                 select new Token(null, a.Position, a.Literal + b.Literal);

            COMMA = Token(",");
        }

        [Test]
        public void CanFailWithoutConsumingInput()
        {
            Fail<string>().FailsToParse(Tokenize("ABC"), "ABC");
        }

        [Test]
        public void CanDetectTheEndOfInputWithoutAdvancing()
        {
            EndOfInput.Parses(Tokenize("")).IntoToken("");
            EndOfInput.FailsToParse(Tokenize("!"), "!").WithMessage("(1, 1): end of input expected");
        }

        [Test]
        public void CanDemandThatAGivenKindOfTokenAppearsNext()
        {
            Token(SampleLexer.Letter).Parses(Tokenize("A")).IntoToken("A");
            Token(SampleLexer.Letter).FailsToParse(Tokenize("0"), "0").WithMessage("(1, 1): Letter expected");

            Token(SampleLexer.Digit).FailsToParse(Tokenize("A"), "A").WithMessage("(1, 1): Digit expected");
            Token(SampleLexer.Digit).Parses(Tokenize("0")).IntoToken("0");
        }

        [Test]
        public void CanDemandThatAGivenTokenLiteralAppearsNext()
        {
            Token("A").Parses(Tokenize("A")).IntoToken("A");
            Token("A").PartiallyParses(Tokenize("A!"), "!").IntoToken("A");
            Token("A").FailsToParse(Tokenize("B"), "B").WithMessage("(1, 1): A expected");
        }

        [Test]
        public void ApplyingARuleFollowedByARequiredButDiscardedTerminatorRule()
        {
            var parser = A.TerminatedBy(B);

            parser.FailsToParse(Tokenize(""), "").WithMessage("(1, 1): A expected");
            parser.FailsToParse(Tokenize("B"), "B").WithMessage("(1, 1): A expected");
            parser.FailsToParse(Tokenize("A"), "").WithMessage("(1, 2): B expected");
            parser.FailsToParse(Tokenize("AA"), "A").WithMessage("(1, 2): B expected");
            parser.Parses(Tokenize("AB")).IntoToken("A");
        }

        [Test]
        public void ApplyingARuleZeroOrMoreTimes()
        {
            var parser = ZeroOrMore(AB);

            parser.Parses(Tokenize("")).IntoTokens();
            parser.PartiallyParses(Tokenize("AB!"), "!").IntoTokens("AB");
            parser.PartiallyParses(Tokenize("ABAB!"), "!").IntoTokens("AB", "AB");
            parser.FailsToParse(Tokenize("ABABA!"), "!").WithMessage("(1, 6): B expected");

            Parser<Token> succeedWithoutConsuming = new LambdaParser<Token>(tokens => new Parsed<Token>(null, tokens));
            Action infiniteLoop = () => ZeroOrMore(succeedWithoutConsuming).Parse(Tokenize(""));
            infiniteLoop.ShouldThrow<Exception>("Parser encountered a potential infinite loop.");
        }

        [Test]
        public void ApplyingARuleOneOrMoreTimes()
        {
            var parser = OneOrMore(AB);

            parser.FailsToParse(Tokenize(""), "").WithMessage("(1, 1): A expected");
            parser.PartiallyParses(Tokenize("AB!"), "!").IntoTokens("AB");
            parser.PartiallyParses(Tokenize("ABAB!"), "!").IntoTokens("AB", "AB");
            parser.FailsToParse(Tokenize("ABABA!"), "!").WithMessage("(1, 6): B expected");

            Parser<Token> succeedWithoutConsuming = new LambdaParser<Token>(tokens => new Parsed<Token>(null, tokens));
            Action infiniteLoop = () => OneOrMore(succeedWithoutConsuming).Parse(Tokenize(""));
            infiniteLoop.ShouldThrow<Exception>("Parser encountered a potential infinite loop.");
        }

        [Test]
        public void ApplyingARuleZeroOrMoreTimesInterspersedByASeparatorRule()
        {
            var parser = ZeroOrMore(AB, COMMA);

            parser.Parses(Tokenize("")).IntoTokens();
            parser.Parses(Tokenize("AB")).IntoTokens("AB");
            parser.Parses(Tokenize("AB,AB")).IntoTokens("AB", "AB");
            parser.Parses(Tokenize("AB,AB,AB")).IntoTokens("AB", "AB", "AB");
            parser.FailsToParse(Tokenize("AB,"), "").WithMessage("(1, 4): A expected");
            parser.FailsToParse(Tokenize("AB,A"), "").WithMessage("(1, 5): B expected");
        }

        [Test]
        public void ApplyingARuleOneOrMoreTimesInterspersedByASeparatorRule()
        {
            var parser = OneOrMore(AB, COMMA);

            parser.FailsToParse(Tokenize(""), "").WithMessage("(1, 1): A expected");
            parser.Parses(Tokenize("AB")).IntoTokens("AB");
            parser.Parses(Tokenize("AB,AB")).IntoTokens("AB", "AB");
            parser.Parses(Tokenize("AB,AB,AB")).IntoTokens("AB", "AB", "AB");
            parser.FailsToParse(Tokenize("AB,"), "").WithMessage("(1, 4): A expected");
            parser.FailsToParse(Tokenize("AB,A"), "").WithMessage("(1, 5): B expected");
        }

        [Test]
        public void ApplyingARuleBetweenTwoOtherRules()
        {
            var parser = Between(A, B, A);

            parser.FailsToParse(Tokenize(""), "").WithMessage("(1, 1): A expected");
            parser.FailsToParse(Tokenize("B"), "B").WithMessage("(1, 1): A expected");
            parser.FailsToParse(Tokenize("A"), "").WithMessage("(1, 2): B expected");
            parser.FailsToParse(Tokenize("AA"), "A").WithMessage("(1, 2): B expected");
            parser.FailsToParse(Tokenize("AB"), "").WithMessage("(1, 3): A expected");
            parser.FailsToParse(Tokenize("ABB"), "B").WithMessage("(1, 3): A expected");
            parser.Parses(Tokenize("ABA")).IntoToken("B");
        }

        [Test]
        public void ParsingAnOptionalRuleZeroOrOneTimes()
        {
            Optional(AB).PartiallyParses(Tokenize("AB."), ".").IntoToken("AB");
            Optional(AB).PartiallyParses(Tokenize("."), ".").IntoValue(token => token.ShouldBeNull());
            Optional(AB).FailsToParse(Tokenize("AC."), "C.").WithMessage("(1, 2): B expected");
        }

        [Test]
        public void AttemptingToParseRuleButBacktrackingUponFailure()
        {
            //When p succeeds, Attempt(p) is the same as p.
            Attempt(AB).Parses(Tokenize("AB")).IntoToken("AB");

            //When p fails without consuming input, Attempt(p) is the same as p.
            Attempt(AB).FailsToParse(Tokenize("!"), "!").WithMessage("(1, 1): A expected");

            //When p fails after consuming input, Attempt(p) backtracks before reporting failure.
            Attempt(AB).FailsToParse(Tokenize("A!"), "A!").WithMessage("(1, 1): [(1, 2): B expected]");
        }

        [Test]
        public void ImprovingDefaultMessagesWithAKnownExpectation()
        {
            var labeled = Label(AB, "'A' followed by 'B'");

            //When p succeeds after consuming input, Label(p) is the same as p.
            AB.Parses(Tokenize("AB")).IntoToken("AB").WithNoMessage();
            labeled.Parses(Tokenize("AB")).IntoToken("AB").WithNoMessage();

            //When p fails after consuming input, Label(p) is the same as p.
            AB.FailsToParse(Tokenize("A!"), "!").WithMessage("(1, 2): B expected");
            labeled.FailsToParse(Tokenize("A!"), "!").WithMessage("(1, 2): B expected");

            //When p succeeds but does not consume input, Label(p) still succeeds but the potential error is included.
            var succeedWithoutConsuming = new Token(null, null, "$").SucceedWithThisValue();
            succeedWithoutConsuming.PartiallyParses(Tokenize("!"), "!").IntoToken("$").WithNoMessage();
            Label(succeedWithoutConsuming, "nothing").PartiallyParses(Tokenize("!"), "!").IntoToken("$").WithMessage("(1, 1): nothing expected");

            //When p fails but does not consume input, Label(p) fails with the given expectation.
            AB.FailsToParse(Tokenize("!"), "!").WithMessage("(1, 1): A expected");
            labeled.FailsToParse(Tokenize("!"), "!").WithMessage("(1, 1): 'A' followed by 'B' expected");
        }
    }

    [TestFixture]
    public class AlternationTests : Grammar
    {
        private static Lexer Tokenize(string source)
        {
            return new CharLexer(source);
        }

        private Parser<Token> A, B, C;

        [SetUp]
        public void Setup()
        {
            A = Token("A");
            B = Token("B");
            C = Token("C");
        }

        [Test]
        public void ChoosingBetweenZeroAlternativesAlwaysFails()
        {
            Choice<string>().FailsToParse(Tokenize("ABC"), "ABC");
        }

        [Test]
        public void ChoosingBetweenOneAlternativeParserIsEquivalentToThatParser()
        {
            Choice(A).Parses(Tokenize("A")).IntoToken("A");
            Choice(A).PartiallyParses(Tokenize("AB"), "B").IntoToken("A");
            Choice(A).FailsToParse(Tokenize("B"), "B").WithMessage("(1, 1): A expected");
        }

        [Test]
        public void FirstParserCanSucceedWithoutExecutingOtherAlternatives()
        {
            Choice(A, NeverExecuted).Parses(Tokenize("A")).IntoToken("A");
        }

        [Test]
        public void SubsequentParserCanSucceedWhenPreviousParsersFailWithoutConsumingInput()
        {
            Choice(B, A).Parses(Tokenize("A")).IntoToken("A");
            Choice(C, B, A).Parses(Tokenize("A")).IntoToken("A");
        }

        [Test]
        public void SubsequentParserWillNotBeAttemptedWhenPreviousParserFailsAfterConsumingInput()
        {
            //As soon as something consumes input, it's failure and message win.

            var AB = from a in A
                     from b in B
                     select new Token(null, a.Position, a.Literal + b.Literal);

            Choice(AB, NeverExecuted).FailsToParse(Tokenize("A"), "").WithMessage("(1, 2): B expected");
            Choice(C, AB, NeverExecuted).FailsToParse(Tokenize("A"), "").WithMessage("(1, 2): B expected");
        }

        [Test]
        public void MergesErrorMessagesWhenParsersFailWithoutConsumingInput()
        {
            Choice(A, B).FailsToParse(Tokenize(""), "").WithMessage("(1, 1): A or B expected");
            Choice(A, B, C).FailsToParse(Tokenize(""), "").WithMessage("(1, 1): A, B or C expected");
        }

        [Test]
        public void MergesPotentialErrorMessagesWhenParserSucceedsWithoutConsumingInput()
        {
            //Choice really shouldn't be used with parsers that can succeed without
            //consuming input.  These tests simply describe the behavior under that
            //unusual situation.

            Parser<Token> succeedWithoutConsuming = new LambdaParser<Token>(tokens => new Parsed<Token>(null, tokens));

            var reply = Choice(A, succeedWithoutConsuming).Parses(Tokenize(""));
            reply.ErrorMessages.ToString().ShouldEqual("A expected");

            reply = Choice(A, B, succeedWithoutConsuming).Parses(Tokenize(""));
            reply.ErrorMessages.ToString().ShouldEqual("A or B expected");

            reply = Choice(A, succeedWithoutConsuming, B).Parses(Tokenize(""));
            reply.ErrorMessages.ToString().ShouldEqual("A expected");
        }

        private static readonly Parser<Token> NeverExecuted = new LambdaParser<Token>(tokens =>
        {
            throw new Exception("Parser 'NeverExecuted' should not have been executed.");
        });
    }
}