using static Parsley.Grammar;

namespace Parsley.Tests;

class GrammarTests
{
    static IEnumerable<Token> Tokenize(string input) => new SampleLexer().Tokenize(input);

    class SampleLexer : Lexer
    {
        public static readonly TokenKind Digit = new Pattern("Digit", @"[0-9]");
        public static readonly TokenKind Letter = new Pattern("Letter", @"[a-zA-Z]");
        public static readonly TokenKind Symbol = new Pattern("Symbol", @".");

        public SampleLexer()
            : base(Digit, Letter, Symbol) { }
    }

    readonly IParser<Token> A, B, AB, COMMA;

    public GrammarTests()
    {
        A = Token("A");
        B = Token("B");

        AB = from a in A
            from b in B
            select new Token(null, a.Position, a.Literal + b.Literal);

        COMMA = Token(",");
    }

    static Action<Token> Literal(string expectedLiteral)
        => t => t.Literal.ShouldBe(expectedLiteral);

    static Action<IEnumerable<Token>> Literals(params string[] expectedLiterals)
        => tokens => tokens.ShouldList(expectedLiterals.Select(Literal).ToArray());

    public void CanFailWithoutConsumingInput()
    {
        Fail<string>().FailsToParse(Tokenize("ABC")).LeavingUnparsedInput("A", "B", "C");
    }

    public void CanDetectTheEndOfInputWithoutAdvancing()
    {
        EndOfInput.Parses(Tokenize("")).WithValue(Literal(""));
        EndOfInput.FailsToParse(Tokenize("!")).LeavingUnparsedInput("!").WithMessage("(1, 1): end of input expected");
    }

    public void CanDemandThatAGivenKindOfTokenAppearsNext()
    {
        Token(SampleLexer.Letter).Parses(Tokenize("A")).WithValue(Literal("A"));
        Token(SampleLexer.Letter).FailsToParse(Tokenize("0")).LeavingUnparsedInput("0").WithMessage("(1, 1): Letter expected");

        Token(SampleLexer.Digit).FailsToParse(Tokenize("A")).LeavingUnparsedInput("A").WithMessage("(1, 1): Digit expected");
        Token(SampleLexer.Digit).Parses(Tokenize("0")).WithValue(Literal("0"));
    }

    public void CanDemandThatAGivenTokenLiteralAppearsNext()
    {
        Token("A").Parses(Tokenize("A")).WithValue(Literal("A"));
        Token("A").PartiallyParses(Tokenize("A!")).LeavingUnparsedInput("!").WithValue(Literal("A"));
        Token("A").FailsToParse(Tokenize("B")).LeavingUnparsedInput("B").WithMessage("(1, 1): A expected");
    }

    public void ApplyingARuleZeroOrMoreTimes()
    {
        var parser = ZeroOrMore(AB);

        parser.Parses(Tokenize("")).Value.ShouldBeEmpty();

        parser.PartiallyParses(Tokenize("AB!"))
            .LeavingUnparsedInput("!")
            .WithValue(Literals("AB"));

        parser.PartiallyParses(Tokenize("ABAB!"))
            .LeavingUnparsedInput("!")
            .WithValue(Literals("AB", "AB"));

        parser.FailsToParse(Tokenize("ABABA!"))
            .LeavingUnparsedInput("!")
            .WithMessage("(1, 6): B expected");

        IParser<Token> succeedWithoutConsuming = new LambdaParser<Token>(tokens => new Parsed<Token>(null, tokens));
        Action infiniteLoop = () => ZeroOrMore(succeedWithoutConsuming).Parse(new Input(Tokenize("")));
        infiniteLoop.ShouldThrow<Exception>("Parser encountered a potential infinite loop at position (1, 1).");
    }

    public void ApplyingARuleOneOrMoreTimes()
    {
        var parser = OneOrMore(AB);

        parser.FailsToParse(Tokenize("")).AtEndOfInput().WithMessage("(1, 1): A expected");

        parser.PartiallyParses(Tokenize("AB!"))
            .LeavingUnparsedInput("!")
            .WithValue(Literals("AB"));

        parser.PartiallyParses(Tokenize("ABAB!"))
            .LeavingUnparsedInput("!")
            .WithValue(Literals("AB", "AB"));

        parser.FailsToParse(Tokenize("ABABA!"))
            .LeavingUnparsedInput("!")
            .WithMessage("(1, 6): B expected");

        IParser<Token> succeedWithoutConsuming = new LambdaParser<Token>(tokens => new Parsed<Token>(null, tokens));
        Action infiniteLoop = () => OneOrMore(succeedWithoutConsuming).Parse(new Input(Tokenize("")));
        infiniteLoop.ShouldThrow<Exception>("Parser encountered a potential infinite loop at position (1, 1).");
    }

    public void ApplyingARuleZeroOrMoreTimesInterspersedByASeparatorRule()
    {
        var parser = ZeroOrMore(AB, COMMA);

        parser.Parses(Tokenize("")).Value.ShouldBeEmpty();
        parser.Parses(Tokenize("AB")).WithValue(Literals("AB"));
        parser.Parses(Tokenize("AB,AB")).WithValue(Literals("AB", "AB"));
        parser.Parses(Tokenize("AB,AB,AB")).WithValue(Literals("AB", "AB", "AB"));
        parser.FailsToParse(Tokenize("AB,")).AtEndOfInput().WithMessage("(1, 4): A expected");
        parser.FailsToParse(Tokenize("AB,A")).AtEndOfInput().WithMessage("(1, 5): B expected");
    }

    public void ApplyingARuleOneOrMoreTimesInterspersedByASeparatorRule()
    {
        var parser = OneOrMore(AB, COMMA);

        parser.FailsToParse(Tokenize("")).AtEndOfInput().WithMessage("(1, 1): A expected");
        parser.Parses(Tokenize("AB")).WithValue(Literals("AB"));
        parser.Parses(Tokenize("AB,AB")).WithValue(Literals("AB", "AB"));
        parser.Parses(Tokenize("AB,AB,AB")).WithValue(Literals("AB", "AB", "AB"));
        parser.FailsToParse(Tokenize("AB,")).AtEndOfInput().WithMessage("(1, 4): A expected");
        parser.FailsToParse(Tokenize("AB,A")).AtEndOfInput().WithMessage("(1, 5): B expected");
    }

    public void ApplyingARuleBetweenTwoOtherRules()
    {
        var parser = Between(A, B, A);

        parser.FailsToParse(Tokenize("")).AtEndOfInput().WithMessage("(1, 1): A expected");
        parser.FailsToParse(Tokenize("B")).LeavingUnparsedInput("B").WithMessage("(1, 1): A expected");
        parser.FailsToParse(Tokenize("A")).AtEndOfInput().WithMessage("(1, 2): B expected");
        parser.FailsToParse(Tokenize("AA")).LeavingUnparsedInput("A").WithMessage("(1, 2): B expected");
        parser.FailsToParse(Tokenize("AB")).AtEndOfInput().WithMessage("(1, 3): A expected");
        parser.FailsToParse(Tokenize("ABB")).LeavingUnparsedInput("B").WithMessage("(1, 3): A expected");
        parser.Parses(Tokenize("ABA")).WithValue(Literal("B"));
    }

    public void ParsingAnOptionalRuleZeroOrOneTimes()
    {
        Optional(AB).PartiallyParses(Tokenize("AB.")).LeavingUnparsedInput(".").WithValue(Literal("AB"));
        Optional(AB).PartiallyParses(Tokenize(".")).LeavingUnparsedInput(".").WithValue(token => token.ShouldBeNull());
        Optional(AB).FailsToParse(Tokenize("AC.")).LeavingUnparsedInput("C", ".").WithMessage("(1, 2): B expected");
    }

    public void AttemptingToParseRuleButBacktrackingUponFailure()
    {
        //When p succeeds, Attempt(p) is the same as p.
        Attempt(AB).Parses(Tokenize("AB")).WithValue(Literal("AB"));

        //When p fails without consuming input, Attempt(p) is the same as p.
        Attempt(AB).FailsToParse(Tokenize("!")).LeavingUnparsedInput("!").WithMessage("(1, 1): A expected");

        //When p fails after consuming input, Attempt(p) backtracks before reporting failure.
        Attempt(AB).FailsToParse(Tokenize("A!")).LeavingUnparsedInput("A", "!").WithMessage("(1, 1): [(1, 2): B expected]");
    }

    public void ImprovingDefaultMessagesWithAKnownExpectation()
    {
        var labeled = Label(AB, "'A' followed by 'B'");

        //When p succeeds after consuming input, Label(p) is the same as p.
        AB.Parses(Tokenize("AB")).WithNoMessage().WithValue(Literal("AB"));
        labeled.Parses(Tokenize("AB")).WithNoMessage().WithValue(Literal("AB"));

        //When p fails after consuming input, Label(p) is the same as p.
        AB.FailsToParse(Tokenize("A!")).LeavingUnparsedInput("!").WithMessage("(1, 2): B expected");
        labeled.FailsToParse(Tokenize("A!")).LeavingUnparsedInput("!").WithMessage("(1, 2): B expected");

        //When p succeeds but does not consume input, Label(p) still succeeds but the potential error is included.
        var succeedWithoutConsuming = new Token(null, null, "$").SucceedWithThisValue();
        succeedWithoutConsuming
            .PartiallyParses(Tokenize("!"))
            .LeavingUnparsedInput("!")
            .WithNoMessage()
            .WithValue(Literal("$"));
        Label(succeedWithoutConsuming, "nothing")
            .PartiallyParses(Tokenize("!"))
            .LeavingUnparsedInput("!")
            .WithMessage("(1, 1): nothing expected")
            .WithValue(Literal("$"));

        //When p fails but does not consume input, Label(p) fails with the given expectation.
        AB.FailsToParse(Tokenize("!")).LeavingUnparsedInput("!").WithMessage("(1, 1): A expected");
        labeled.FailsToParse(Tokenize("!")).LeavingUnparsedInput("!").WithMessage("(1, 1): 'A' followed by 'B' expected");
    }
}

public class AlternationTests
{
    static IEnumerable<Token> Tokenize(string input) => new CharLexer().Tokenize(input);

    readonly IParser<Token> A, B, C;

    public AlternationTests()
    {
        A = Token("A");
        B = Token("B");
        C = Token("C");
    }

    static Action<Token> Literal(string expectedLiteral)
        => t => t.Literal.ShouldBe(expectedLiteral);

    public void ChoosingBetweenZeroAlternativesAlwaysFails()
    {
        Choice<string>().FailsToParse(Tokenize("ABC")).LeavingUnparsedInput("A", "B", "C");
    }

    public void ChoosingBetweenOneAlternativeParserIsEquivalentToThatParser()
    {
        Choice(A).Parses(Tokenize("A")).WithValue(Literal("A"));
        Choice(A).PartiallyParses(Tokenize("AB")).LeavingUnparsedInput("B").WithValue(Literal("A"));
        Choice(A).FailsToParse(Tokenize("B")).LeavingUnparsedInput("B").WithMessage("(1, 1): A expected");
    }

    public void FirstParserCanSucceedWithoutExecutingOtherAlternatives()
    {
        Choice(A, NeverExecuted).Parses(Tokenize("A")).WithValue(Literal("A"));
    }

    public void SubsequentParserCanSucceedWhenPreviousParsersFailWithoutConsumingInput()
    {
        Choice(B, A).Parses(Tokenize("A")).WithValue(Literal("A"));
        Choice(C, B, A).Parses(Tokenize("A")).WithValue(Literal("A"));
    }

    public void SubsequentParserWillNotBeAttemptedWhenPreviousParserFailsAfterConsumingInput()
    {
        //As soon as something consumes input, it's failure and message win.

        var AB = from a in A
            from b in B
            select new Token(null, a.Position, a.Literal + b.Literal);

        Choice(AB, NeverExecuted).FailsToParse(Tokenize("A")).AtEndOfInput().WithMessage("(1, 2): B expected");
        Choice(C, AB, NeverExecuted).FailsToParse(Tokenize("A")).AtEndOfInput().WithMessage("(1, 2): B expected");
    }

    public void MergesErrorMessagesWhenParsersFailWithoutConsumingInput()
    {
        Choice(A, B).FailsToParse(Tokenize("")).AtEndOfInput().WithMessage("(1, 1): A or B expected");
        Choice(A, B, C).FailsToParse(Tokenize("")).AtEndOfInput().WithMessage("(1, 1): A, B or C expected");
    }

    public void MergesPotentialErrorMessagesWhenParserSucceedsWithoutConsumingInput()
    {
        //Choice really shouldn't be used with parsers that can succeed without
        //consuming input.  These tests simply describe the behavior under that
        //unusual situation.

        IParser<Token> succeedWithoutConsuming = new LambdaParser<Token>(tokens => new Parsed<Token>(null, tokens));

        var reply = Choice(A, succeedWithoutConsuming).Parses(Tokenize(""));
        reply.ErrorMessages.ToString().ShouldBe("A expected");

        reply = Choice(A, B, succeedWithoutConsuming).Parses(Tokenize(""));
        reply.ErrorMessages.ToString().ShouldBe("A or B expected");

        reply = Choice(A, succeedWithoutConsuming, B).Parses(Tokenize(""));
        reply.ErrorMessages.ToString().ShouldBe("A expected");
    }

    static readonly IParser<Token> NeverExecuted = new LambdaParser<Token>(tokens =>
    {
        throw new Exception("Parser 'NeverExecuted' should not have been executed.");
    });
}

