using static Parsley.Grammar;

namespace Parsley.Tests;

class GrammarTests
{
    static readonly Pattern Digit = new("Digit", @"[0-9]");
    static readonly Pattern Letter = new("Letter", @"[a-zA-Z]");
    static readonly Pattern Symbol = new("Symbol", @".");

    readonly IParser<Token> A, B, AB, COMMA;

    public GrammarTests()
    {
        A = Token(Letter, "A");
        B = Token(Letter, "B");

        AB = from a in A
            from b in B
            select new Token(null, a.Literal + b.Literal);

        COMMA = Token(Symbol, ",");
    }

    static Action<Token> Literal(string expectedLiteral)
        => t => t.Literal.ShouldBe(expectedLiteral);

    static Action<IEnumerable<Token>> Literals(params string[] expectedLiterals)
        => tokens => tokens.ShouldList(expectedLiterals.Select(Literal).ToArray());

    public void CanFailWithoutConsumingInput()
    {
        Fail<string>().FailsToParse("ABC").LeavingUnparsedInput("ABC");
    }

    public void CanDetectTheEndOfInputWithoutAdvancing()
    {
        EndOfInput.Parses("").WithValue(Literal(""));
        EndOfInput.FailsToParse("!").LeavingUnparsedInput("!").WithMessage("(1, 1): end of input expected");
    }

    public void CanDemandThatAGivenKindOfTokenAppearsNext()
    {
        Token(Letter).Parses("A").WithValue(Literal("A"));
        Token(Letter).FailsToParse("0").LeavingUnparsedInput("0").WithMessage("(1, 1): Letter expected");

        Token(Digit).FailsToParse("A").LeavingUnparsedInput("A").WithMessage("(1, 1): Digit expected");
        Token(Digit).Parses("0").WithValue(Literal("0"));
    }

    public void CanDemandThatAGivenTokenLiteralAppearsNext()
    {
        Token(Letter, "A").Parses("A").WithValue(Literal("A"));
        Token(Letter, "A").PartiallyParses("A!").LeavingUnparsedInput("!").WithValue(Literal("A"));
        Token(Letter, "A").FailsToParse("B").LeavingUnparsedInput("B").WithMessage("(1, 1): A expected");
    }

    public void ApplyingARuleZeroOrMoreTimes()
    {
        var parser = ZeroOrMore(AB);

        parser.Parses("").Value.ShouldBeEmpty();

        parser.PartiallyParses("AB!")
            .LeavingUnparsedInput("!")
            .WithValue(Literals("AB"));

        parser.PartiallyParses("ABAB!")
            .LeavingUnparsedInput("!")
            .WithValue(Literals("AB", "AB"));

        parser.FailsToParse("ABABA!")
            .LeavingUnparsedInput("!")
            .WithMessage("(1, 6): B expected");

        IParser<Token> succeedWithoutConsuming = new LambdaParser<Token>(tokens => new Parsed<Token>(null, tokens));
        Action infiniteLoop = () => ZeroOrMore(succeedWithoutConsuming).Parse(new Text(""));
        infiniteLoop.ShouldThrow<Exception>("Parser encountered a potential infinite loop at position (1, 1).");
    }

    public void ApplyingARuleOneOrMoreTimes()
    {
        var parser = OneOrMore(AB);

        parser.FailsToParse("").AtEndOfInput().WithMessage("(1, 1): A expected");

        parser.PartiallyParses("AB!")
            .LeavingUnparsedInput("!")
            .WithValue(Literals("AB"));

        parser.PartiallyParses("ABAB!")
            .LeavingUnparsedInput("!")
            .WithValue(Literals("AB", "AB"));

        parser.FailsToParse("ABABA!")
            .LeavingUnparsedInput("!")
            .WithMessage("(1, 6): B expected");

        IParser<Token> succeedWithoutConsuming = new LambdaParser<Token>(tokens => new Parsed<Token>(null, tokens));
        Action infiniteLoop = () => OneOrMore(succeedWithoutConsuming).Parse(new Text(""));
        infiniteLoop.ShouldThrow<Exception>("Parser encountered a potential infinite loop at position (1, 1).");
    }

    public void ApplyingARuleZeroOrMoreTimesInterspersedByASeparatorRule()
    {
        var parser = ZeroOrMore(AB, COMMA);

        parser.Parses("").Value.ShouldBeEmpty();
        parser.Parses("AB").WithValue(Literals("AB"));
        parser.Parses("AB,AB").WithValue(Literals("AB", "AB"));
        parser.Parses("AB,AB,AB").WithValue(Literals("AB", "AB", "AB"));
        parser.FailsToParse("AB,").AtEndOfInput().WithMessage("(1, 4): A expected");
        parser.FailsToParse("AB,A").AtEndOfInput().WithMessage("(1, 5): B expected");
    }

    public void ApplyingARuleOneOrMoreTimesInterspersedByASeparatorRule()
    {
        var parser = OneOrMore(AB, COMMA);

        parser.FailsToParse("").AtEndOfInput().WithMessage("(1, 1): A expected");
        parser.Parses("AB").WithValue(Literals("AB"));
        parser.Parses("AB,AB").WithValue(Literals("AB", "AB"));
        parser.Parses("AB,AB,AB").WithValue(Literals("AB", "AB", "AB"));
        parser.FailsToParse("AB,").AtEndOfInput().WithMessage("(1, 4): A expected");
        parser.FailsToParse("AB,A").AtEndOfInput().WithMessage("(1, 5): B expected");
    }

    public void ApplyingARuleBetweenTwoOtherRules()
    {
        var parser = Between(A, B, A);

        parser.FailsToParse("").AtEndOfInput().WithMessage("(1, 1): A expected");
        parser.FailsToParse("B").LeavingUnparsedInput("B").WithMessage("(1, 1): A expected");
        parser.FailsToParse("A").AtEndOfInput().WithMessage("(1, 2): B expected");
        parser.FailsToParse("AA").LeavingUnparsedInput("A").WithMessage("(1, 2): B expected");
        parser.FailsToParse("AB").AtEndOfInput().WithMessage("(1, 3): A expected");
        parser.FailsToParse("ABB").LeavingUnparsedInput("B").WithMessage("(1, 3): A expected");
        parser.Parses("ABA").WithValue(Literal("B"));
    }

    public void ParsingAnOptionalRuleZeroOrOneTimes()
    {
        Optional(AB).PartiallyParses("AB.").LeavingUnparsedInput(".").WithValue(Literal("AB"));
        Optional(AB).PartiallyParses(".").LeavingUnparsedInput(".").WithValue(token => token.ShouldBeNull());
        Optional(AB).FailsToParse("AC.").LeavingUnparsedInput("C.").WithMessage("(1, 2): B expected");
    }

    public void AttemptingToParseRuleButBacktrackingUponFailure()
    {
        //When p succeeds, Attempt(p) is the same as p.
        Attempt(AB).Parses("AB").WithValue(Literal("AB"));

        //When p fails without consuming input, Attempt(p) is the same as p.
        Attempt(AB).FailsToParse("!").LeavingUnparsedInput("!").WithMessage("(1, 1): A expected");

        //When p fails after consuming input, Attempt(p) backtracks before reporting failure.
        Attempt(AB).FailsToParse("A!").LeavingUnparsedInput("A!").WithMessage("(1, 1): [(1, 2): B expected]");
    }

    public void ImprovingDefaultMessagesWithAKnownExpectation()
    {
        var labeled = Label(AB, "'A' followed by 'B'");

        //When p succeeds after consuming input, Label(p) is the same as p.
        AB.Parses("AB").WithNoMessage().WithValue(Literal("AB"));
        labeled.Parses("AB").WithNoMessage().WithValue(Literal("AB"));

        //When p fails after consuming input, Label(p) is the same as p.
        AB.FailsToParse("A!").LeavingUnparsedInput("!").WithMessage("(1, 2): B expected");
        labeled.FailsToParse("A!").LeavingUnparsedInput("!").WithMessage("(1, 2): B expected");

        //When p succeeds but does not consume input, Label(p) still succeeds but the potential error is included.
        var succeedWithoutConsuming = new Token(null, "$").SucceedWithThisValue();
        succeedWithoutConsuming
            .PartiallyParses("!")
            .LeavingUnparsedInput("!")
            .WithNoMessage()
            .WithValue(Literal("$"));
        Label(succeedWithoutConsuming, "nothing")
            .PartiallyParses("!")
            .LeavingUnparsedInput("!")
            .WithMessage("(1, 1): nothing expected")
            .WithValue(Literal("$"));

        //When p fails but does not consume input, Label(p) fails with the given expectation.
        AB.FailsToParse("!").LeavingUnparsedInput("!").WithMessage("(1, 1): A expected");
        labeled.FailsToParse("!").LeavingUnparsedInput("!").WithMessage("(1, 1): 'A' followed by 'B' expected");
    }
}

public class AlternationTests
{
    readonly IParser<Token> A, B, C;

    public AlternationTests()
    {
        var kind = new Pattern("Character", @".");
        A = Token(kind, "A");
        B = Token(kind, "B");
        C = Token(kind, "C");
    }

    static Action<Token> Literal(string expectedLiteral)
        => t => t.Literal.ShouldBe(expectedLiteral);

    public void ChoosingBetweenZeroAlternativesAlwaysFails()
    {
        Choice<string>().FailsToParse("ABC").LeavingUnparsedInput("ABC");
    }

    public void ChoosingBetweenOneAlternativeParserIsEquivalentToThatParser()
    {
        Choice(A).Parses("A").WithValue(Literal("A"));
        Choice(A).PartiallyParses("AB").LeavingUnparsedInput("B").WithValue(Literal("A"));
        Choice(A).FailsToParse("B").LeavingUnparsedInput("B").WithMessage("(1, 1): A expected");
    }

    public void FirstParserCanSucceedWithoutExecutingOtherAlternatives()
    {
        Choice(A, NeverExecuted).Parses("A").WithValue(Literal("A"));
    }

    public void SubsequentParserCanSucceedWhenPreviousParsersFailWithoutConsumingInput()
    {
        Choice(B, A).Parses("A").WithValue(Literal("A"));
        Choice(C, B, A).Parses("A").WithValue(Literal("A"));
    }

    public void SubsequentParserWillNotBeAttemptedWhenPreviousParserFailsAfterConsumingInput()
    {
        //As soon as something consumes input, it's failure and message win.

        var AB = from a in A
            from b in B
            select new Token(null, a.Literal + b.Literal);

        Choice(AB, NeverExecuted).FailsToParse("A").AtEndOfInput().WithMessage("(1, 2): B expected");
        Choice(C, AB, NeverExecuted).FailsToParse("A").AtEndOfInput().WithMessage("(1, 2): B expected");
    }

    public void MergesErrorMessagesWhenParsersFailWithoutConsumingInput()
    {
        Choice(A, B).FailsToParse("").AtEndOfInput().WithMessage("(1, 1): A or B expected");
        Choice(A, B, C).FailsToParse("").AtEndOfInput().WithMessage("(1, 1): A, B or C expected");
    }

    public void MergesPotentialErrorMessagesWhenParserSucceedsWithoutConsumingInput()
    {
        //Choice really shouldn't be used with parsers that can succeed without
        //consuming input.  These tests simply describe the behavior under that
        //unusual situation.

        IParser<Token> succeedWithoutConsuming = new LambdaParser<Token>(tokens => new Parsed<Token>(null, tokens));

        var reply = Choice(A, succeedWithoutConsuming).Parses("");
        reply.ErrorMessages.ToString().ShouldBe("A expected");

        reply = Choice(A, B, succeedWithoutConsuming).Parses("");
        reply.ErrorMessages.ToString().ShouldBe("A or B expected");

        reply = Choice(A, succeedWithoutConsuming, B).Parses("");
        reply.ErrorMessages.ToString().ShouldBe("A expected");
    }

    static readonly IParser<Token> NeverExecuted = new LambdaParser<Token>(tokens =>
    {
        throw new Exception("Parser 'NeverExecuted' should not have been executed.");
    });
}
