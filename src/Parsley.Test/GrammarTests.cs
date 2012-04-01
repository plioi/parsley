using System;
using System.Collections.Generic;
using Should;
using Xunit;

namespace Parsley
{
    public class GrammarTests : Grammar
    {
        private static IEnumerable<Token> Tokenize(string input)
        {
            return new SampleLexer().Tokenize(input);
        }

        private class SampleLexer : Lexer
        {
            public static readonly TokenKind Digit = new Pattern("Digit", @"[0-9]");
            public static readonly TokenKind Letter = new Pattern("Letter", @"[a-zA-Z]");
            public static readonly TokenKind Symbol = new Pattern("Symbol", @".");

            public SampleLexer()
                : base(Digit, Letter, Symbol) { }
        }

        private readonly Parser<Token> A, B, AB, COMMA;

        public GrammarTests()
        {
            A = Token("A");
            B = Token("B");

            AB = from a in A
                 from b in B
                 select new Token(null, a.Position, a.Literal + b.Literal);

            COMMA = Token(",");
        }

        [Fact]
        public void CanFailWithoutConsumingInput()
        {
            Fail<string>().FailsToParse(Tokenize("ABC")).LeavingUnparsedTokens("A", "B", "C");
        }

        [Fact]
        public void CanDetectTheEndOfInputWithoutAdvancing()
        {
            EndOfInput.Parses(Tokenize("")).IntoToken("");
            EndOfInput.FailsToParse(Tokenize("!")).LeavingUnparsedTokens("!").WithMessage("(1, 1): end of input expected");
        }

        [Fact]
        public void CanDemandThatAGivenKindOfTokenAppearsNext()
        {
            Token(SampleLexer.Letter).Parses(Tokenize("A")).IntoToken("A");
            Token(SampleLexer.Letter).FailsToParse(Tokenize("0")).LeavingUnparsedTokens("0").WithMessage("(1, 1): Letter expected");

            Token(SampleLexer.Digit).FailsToParse(Tokenize("A")).LeavingUnparsedTokens("A").WithMessage("(1, 1): Digit expected");
            Token(SampleLexer.Digit).Parses(Tokenize("0")).IntoToken("0");
        }

        [Fact]
        public void CanDemandThatAGivenTokenLiteralAppearsNext()
        {
            Token("A").Parses(Tokenize("A")).IntoToken("A");
            Token("A").PartiallyParses(Tokenize("A!")).LeavingUnparsedTokens("!").IntoToken("A");
            Token("A").FailsToParse(Tokenize("B")).LeavingUnparsedTokens("B").WithMessage("(1, 1): A expected");
        }

        [Fact]
        public void ApplyingARuleZeroOrMoreTimes()
        {
            var parser = ZeroOrMore(AB);

            parser.Parses(Tokenize("")).IntoTokens();
            parser.PartiallyParses(Tokenize("AB!")).LeavingUnparsedTokens("!").IntoTokens("AB");
            parser.PartiallyParses(Tokenize("ABAB!")).LeavingUnparsedTokens("!").IntoTokens("AB", "AB");
            parser.FailsToParse(Tokenize("ABABA!")).LeavingUnparsedTokens("!").WithMessage("(1, 6): B expected");

            Parser<Token> succeedWithoutConsuming = new LambdaParser<Token>(tokens => new Parsed<Token>(null, tokens));
            Action infiniteLoop = () => ZeroOrMore(succeedWithoutConsuming).Parse(new TokenStream(Tokenize("")));
            infiniteLoop.ShouldThrow<Exception>("Parser encountered a potential infinite loop.");
        }

        [Fact]
        public void ApplyingARuleOneOrMoreTimes()
        {
            var parser = OneOrMore(AB);

            parser.FailsToParse(Tokenize("")).AtEndOfInput().WithMessage("(1, 1): A expected");
            parser.PartiallyParses(Tokenize("AB!")).LeavingUnparsedTokens("!").IntoTokens("AB");
            parser.PartiallyParses(Tokenize("ABAB!")).LeavingUnparsedTokens("!").IntoTokens("AB", "AB");
            parser.FailsToParse(Tokenize("ABABA!")).LeavingUnparsedTokens("!").WithMessage("(1, 6): B expected");

            Parser<Token> succeedWithoutConsuming = new LambdaParser<Token>(tokens => new Parsed<Token>(null, tokens));
            Action infiniteLoop = () => OneOrMore(succeedWithoutConsuming).Parse(new TokenStream(Tokenize("")));
            infiniteLoop.ShouldThrow<Exception>("Parser encountered a potential infinite loop.");
        }

        [Fact]
        public void ApplyingARuleZeroOrMoreTimesInterspersedByASeparatorRule()
        {
            var parser = ZeroOrMore(AB, COMMA);

            parser.Parses(Tokenize("")).IntoTokens();
            parser.Parses(Tokenize("AB")).IntoTokens("AB");
            parser.Parses(Tokenize("AB,AB")).IntoTokens("AB", "AB");
            parser.Parses(Tokenize("AB,AB,AB")).IntoTokens("AB", "AB", "AB");
            parser.FailsToParse(Tokenize("AB,")).AtEndOfInput().WithMessage("(1, 4): A expected");
            parser.FailsToParse(Tokenize("AB,A")).AtEndOfInput().WithMessage("(1, 5): B expected");
        }

        [Fact]
        public void ApplyingARuleOneOrMoreTimesInterspersedByASeparatorRule()
        {
            var parser = OneOrMore(AB, COMMA);

            parser.FailsToParse(Tokenize("")).AtEndOfInput().WithMessage("(1, 1): A expected");
            parser.Parses(Tokenize("AB")).IntoTokens("AB");
            parser.Parses(Tokenize("AB,AB")).IntoTokens("AB", "AB");
            parser.Parses(Tokenize("AB,AB,AB")).IntoTokens("AB", "AB", "AB");
            parser.FailsToParse(Tokenize("AB,")).AtEndOfInput().WithMessage("(1, 4): A expected");
            parser.FailsToParse(Tokenize("AB,A")).AtEndOfInput().WithMessage("(1, 5): B expected");
        }

        [Fact]
        public void ApplyingARuleBetweenTwoOtherRules()
        {
            var parser = Between(A, B, A);

            parser.FailsToParse(Tokenize("")).AtEndOfInput().WithMessage("(1, 1): A expected");
            parser.FailsToParse(Tokenize("B")).LeavingUnparsedTokens("B").WithMessage("(1, 1): A expected");
            parser.FailsToParse(Tokenize("A")).AtEndOfInput().WithMessage("(1, 2): B expected");
            parser.FailsToParse(Tokenize("AA")).LeavingUnparsedTokens("A").WithMessage("(1, 2): B expected");
            parser.FailsToParse(Tokenize("AB")).AtEndOfInput().WithMessage("(1, 3): A expected");
            parser.FailsToParse(Tokenize("ABB")).LeavingUnparsedTokens("B").WithMessage("(1, 3): A expected");
            parser.Parses(Tokenize("ABA")).IntoToken("B");
        }

        [Fact]
        public void ParsingAnOptionalRuleZeroOrOneTimes()
        {
            Optional(AB).PartiallyParses(Tokenize("AB.")).LeavingUnparsedTokens(".").IntoToken("AB");
            Optional(AB).PartiallyParses(Tokenize(".")).LeavingUnparsedTokens(".").IntoValue(token => token.ShouldBeNull());
            Optional(AB).FailsToParse(Tokenize("AC.")).LeavingUnparsedTokens("C", ".").WithMessage("(1, 2): B expected");
        }

        [Fact]
        public void AttemptingToParseRuleButBacktrackingUponFailure()
        {
            //When p succeeds, Attempt(p) is the same as p.
            Attempt(AB).Parses(Tokenize("AB")).IntoToken("AB");

            //When p fails without consuming input, Attempt(p) is the same as p.
            Attempt(AB).FailsToParse(Tokenize("!")).LeavingUnparsedTokens("!").WithMessage("(1, 1): A expected");

            //When p fails after consuming input, Attempt(p) backtracks before reporting failure.
            Attempt(AB).FailsToParse(Tokenize("A!")).LeavingUnparsedTokens("A", "!").WithMessage("(1, 1): [(1, 2): B expected]");
        }

        [Fact]
        public void ImprovingDefaultMessagesWithAKnownExpectation()
        {
            var labeled = Label(AB, "'A' followed by 'B'");

            //When p succeeds after consuming input, Label(p) is the same as p.
            AB.Parses(Tokenize("AB")).IntoToken("AB").WithNoMessage();
            labeled.Parses(Tokenize("AB")).IntoToken("AB").WithNoMessage();

            //When p fails after consuming input, Label(p) is the same as p.
            AB.FailsToParse(Tokenize("A!")).LeavingUnparsedTokens("!").WithMessage("(1, 2): B expected");
            labeled.FailsToParse(Tokenize("A!")).LeavingUnparsedTokens("!").WithMessage("(1, 2): B expected");

            //When p succeeds but does not consume input, Label(p) still succeeds but the potential error is included.
            var succeedWithoutConsuming = new Token(null, null, "$").SucceedWithThisValue();
            succeedWithoutConsuming.PartiallyParses(Tokenize("!")).IntoToken("$").LeavingUnparsedTokens("!").WithNoMessage();
            Label(succeedWithoutConsuming, "nothing").PartiallyParses(Tokenize("!")).IntoToken("$").LeavingUnparsedTokens("!").WithMessage("(1, 1): nothing expected");

            //When p fails but does not consume input, Label(p) fails with the given expectation.
            AB.FailsToParse(Tokenize("!")).LeavingUnparsedTokens("!").WithMessage("(1, 1): A expected");
            labeled.FailsToParse(Tokenize("!")).LeavingUnparsedTokens("!").WithMessage("(1, 1): 'A' followed by 'B' expected");
        }
    }

    public class AlternationTests : Grammar
    {
        private static IEnumerable<Token> Tokenize(string input)
        {
            return new CharLexer().Tokenize(input);
        }

        private readonly Parser<Token> A, B, C;

        public AlternationTests()
        {
            A = Token("A");
            B = Token("B");
            C = Token("C");
        }

        [Fact]
        public void ChoosingBetweenZeroAlternativesAlwaysFails()
        {
            Choice<string>().FailsToParse(Tokenize("ABC")).LeavingUnparsedTokens("A", "B", "C");
        }

        [Fact]
        public void ChoosingBetweenOneAlternativeParserIsEquivalentToThatParser()
        {
            Choice(A).Parses(Tokenize("A")).IntoToken("A");
            Choice(A).PartiallyParses(Tokenize("AB")).IntoToken("A").LeavingUnparsedTokens("B");
            Choice(A).FailsToParse(Tokenize("B")).LeavingUnparsedTokens("B").WithMessage("(1, 1): A expected");
        }

        [Fact]
        public void FirstParserCanSucceedWithoutExecutingOtherAlternatives()
        {
            Choice(A, NeverExecuted).Parses(Tokenize("A")).IntoToken("A");
        }

        [Fact]
        public void SubsequentParserCanSucceedWhenPreviousParsersFailWithoutConsumingInput()
        {
            Choice(B, A).Parses(Tokenize("A")).IntoToken("A");
            Choice(C, B, A).Parses(Tokenize("A")).IntoToken("A");
        }

        [Fact]
        public void SubsequentParserWillNotBeAttemptedWhenPreviousParserFailsAfterConsumingInput()
        {
            //As soon as something consumes input, it's failure and message win.

            var AB = from a in A
                     from b in B
                     select new Token(null, a.Position, a.Literal + b.Literal);

            Choice(AB, NeverExecuted).FailsToParse(Tokenize("A")).AtEndOfInput().WithMessage("(1, 2): B expected");
            Choice(C, AB, NeverExecuted).FailsToParse(Tokenize("A")).AtEndOfInput().WithMessage("(1, 2): B expected");
        }

        [Fact]
        public void MergesErrorMessagesWhenParsersFailWithoutConsumingInput()
        {
            Choice(A, B).FailsToParse(Tokenize("")).AtEndOfInput().WithMessage("(1, 1): A or B expected");
            Choice(A, B, C).FailsToParse(Tokenize("")).AtEndOfInput().WithMessage("(1, 1): A, B or C expected");
        }

        [Fact]
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

    public class GrammarRuleNameInferenceTests : Grammar
    {
        private readonly GrammarRule<int> AlreadyNamedRule;
        public static GrammarRule<object> PublicStaticRule;
        private static GrammarRule<string> PrivateStaticRule;
        public readonly GrammarRule<int> PublicInstanceRule;
        private readonly GrammarRule<int> PrivateInstanceRule;
        private readonly GrammarRule<int> NullRule;

        public GrammarRuleNameInferenceTests()
        {
            AlreadyNamedRule = new GrammarRule<int>("This name is not inferred.");
            PublicStaticRule = new GrammarRule<object>();
            PrivateStaticRule = new GrammarRule<string>();
            PublicInstanceRule = new GrammarRule<int>();
            PrivateInstanceRule = new GrammarRule<int>();
            NullRule = null;

            InferGrammarRuleNames();
        }

        [Fact]
        public void WillNotInferNameWhenNameIsAlreadyProvided()
        {
            AlreadyNamedRule.Name.ShouldEqual("This name is not inferred.");
        }

        [Fact]
        public void InfersNamesOfPublicStaticGrammarRules()
        {
            PublicStaticRule.Name.ShouldEqual("PublicStaticRule");
        }

        [Fact]
        public void InfersNamesOfPrivateStaticGrammarRules()
        {
            PrivateStaticRule.Name.ShouldEqual("PrivateStaticRule");
        }

        [Fact]
        public void InfersNamesOfPublicInstanceGrammarRules()
        {
            PublicInstanceRule.Name.ShouldEqual("PublicInstanceRule");
        }

        [Fact]
        public void InfersNamesOfPrivateInstanceGrammarRules()
        {
            PrivateInstanceRule.Name.ShouldEqual("PrivateInstanceRule");
        }

        [Fact]
        public void SilentlyIgnoresNullRules()
        {
            NullRule.ShouldBeNull();
        }
    }
}