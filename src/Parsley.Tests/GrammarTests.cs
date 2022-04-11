using System.Text.RegularExpressions;
using static Parsley.Grammar;

namespace Parsley.Tests;

class GrammarTests
{
    static readonly IParser<string> Digit = Pattern("Digit", @"[0-9]");
    static readonly IParser<string> Letter = Pattern("Letter", @"[a-zA-Z]");

    readonly IParser<string> A, B, AB, COMMA;

    public GrammarTests()
    {
        A = Pattern("A", @"A");
        B = Pattern("B", @"B");

        AB = from a in A
            from b in B
            select a + b;

        COMMA = Pattern("COMMA", @",");
    }

    static Action<IEnumerable<string>> Literals(params string[] expectedLiterals)
        => actualLiterals => actualLiterals.ShouldBe(expectedLiterals);

    public void CanFailWithoutConsumingInput()
    {
        Fail<string>().FailsToParse("ABC").LeavingUnparsedInput("ABC");
    }

    public void CanDetectTheEndOfInputWithoutAdvancing()
    {
        EndOfInput.Parses("").WithValue("");
        EndOfInput.FailsToParse("!").LeavingUnparsedInput("!").WithMessage("(1, 1): end of input expected");
    }

    public void CanDemandThatAGivenParserRecognizesTheNextConsumableInput()
    {
        Letter.Parses("A").WithValue("A");
        Letter.FailsToParse("0").LeavingUnparsedInput("0").WithMessage("(1, 1): Letter expected");

        Digit.FailsToParse("A").LeavingUnparsedInput("A").WithMessage("(1, 1): Digit expected");
        Digit.Parses("0").WithValue("0");
    }

    public void CanDemandThatAGivenTokenLiteralAppearsNext()
    {
        A.Parses("A").WithValue("A");
        A.PartiallyParses("A!").LeavingUnparsedInput("!").WithValue("A");
        A.FailsToParse("B").LeavingUnparsedInput("B").WithMessage("(1, 1): A expected");
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

        IParser<string> succeedWithoutConsuming = new LambdaParser<string>(input => new Parsed<string>(null, input));
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

        IParser<string> succeedWithoutConsuming = new LambdaParser<string>(input => new Parsed<string>(null, input));
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
        parser.Parses("ABA").WithValue("B");
    }

    public void ParsingAnOptionalRuleZeroOrOneTimes()
    {
        Optional(AB).PartiallyParses("AB.").LeavingUnparsedInput(".").WithValue("AB");
        Optional(AB).PartiallyParses(".").LeavingUnparsedInput(".").WithValue((string)null);
        Optional(AB).FailsToParse("AC.").LeavingUnparsedInput("C.").WithMessage("(1, 2): B expected");
    }

    public void AttemptingToParseRuleButBacktrackingUponFailure()
    {
        //When p succeeds, Attempt(p) is the same as p.
        Attempt(AB).Parses("AB").WithValue("AB");

        //When p fails without consuming input, Attempt(p) is the same as p.
        Attempt(AB).FailsToParse("!").LeavingUnparsedInput("!").WithMessage("(1, 1): A expected");

        //When p fails after consuming input, Attempt(p) backtracks before reporting failure.
        Attempt(AB).FailsToParse("A!").LeavingUnparsedInput("A!").WithMessage("(1, 1): [(1, 2): B expected]");
    }

    public void ImprovingDefaultMessagesWithAKnownExpectation()
    {
        var labeled = Label(AB, "'A' followed by 'B'");

        //When p succeeds after consuming input, Label(p) is the same as p.
        AB.Parses("AB").WithNoMessage().WithValue("AB");
        labeled.Parses("AB").WithNoMessage().WithValue("AB");

        //When p fails after consuming input, Label(p) is the same as p.
        AB.FailsToParse("A!").LeavingUnparsedInput("!").WithMessage("(1, 2): B expected");
        labeled.FailsToParse("A!").LeavingUnparsedInput("!").WithMessage("(1, 2): B expected");

        //When p succeeds but does not consume input, Label(p) still succeeds but the potential error is included.
        var succeedWithoutConsuming = "$".SucceedWithThisValue();
        succeedWithoutConsuming
            .PartiallyParses("!")
            .LeavingUnparsedInput("!")
            .WithNoMessage()
            .WithValue("$");
        Label(succeedWithoutConsuming, "nothing")
            .PartiallyParses("!")
            .LeavingUnparsedInput("!")
            .WithMessage("(1, 1): nothing expected")
            .WithValue("$");

        //When p fails but does not consume input, Label(p) fails with the given expectation.
        AB.FailsToParse("!").LeavingUnparsedInput("!").WithMessage("(1, 1): A expected");
        labeled.FailsToParse("!").LeavingUnparsedInput("!").WithMessage("(1, 1): 'A' followed by 'B' expected");
    }

    public void ProvidesConveniencePrimitiveForRecognizingNamedRegexPatterns()
    {
        var lower = Pattern("Lowercase", @"[a-z]+");
        var upper = Pattern("Uppercase", @"[A-Z]+");
        var caseInsensitive = Pattern("Case Insensitive", @"[a-z]+", RegexOptions.IgnoreCase);

        lower.FailsToParse("ABCdef")
            .LeavingUnparsedInput("ABCdef")
            .WithMessage("(1, 1): Lowercase expected");

        upper.FailsToParse("abcDEF")
            .LeavingUnparsedInput("abcDEF")
            .WithMessage("(1, 1): Uppercase expected");

        caseInsensitive.FailsToParse("!abcDEF")
            .LeavingUnparsedInput("!abcDEF")
            .WithMessage("(1, 1): Case Insensitive expected");

        lower.PartiallyParses("abcDEF")
            .LeavingUnparsedInput("DEF")
            .WithValue("abc");

        upper.Parses("DEF")
            .WithValue("DEF");

        caseInsensitive.Parses("abcDEF")
            .WithValue("abcDEF");
    }

    public void ProvidesConveniencePrimitiveForDefiningKeywords()
    {
        var foo = Keyword("foo");

        foo.FailsToParse("bar")
            .LeavingUnparsedInput("bar")
            .WithMessage("(1, 1): foo expected");

        foo.Parses("foo")
            .WithValue("foo");

        foo.PartiallyParses("foo bar")
            .LeavingUnparsedInput(" bar")
            .WithValue("foo");

        foo.FailsToParse("foobar")
            .LeavingUnparsedInput("foobar")
            .WithMessage("(1, 1): foo expected");

        var notJustLetters = () => Keyword(" oops ");
        notJustLetters.ShouldThrow<ArgumentException>("Keywords may only contain letters. (Parameter 'word')");
    }

    public void ProvidesConveniencePrimitiveForDefiningOperators()
    {
        var star = Operator("*");
        var doubleStar = Operator("**");

        star.FailsToParse("a")
            .LeavingUnparsedInput("a")
            .WithMessage("(1, 1): * expected");

        star.Parses("*")
            .WithValue("*");

        star.PartiallyParses("* *")
            .LeavingUnparsedInput(" *")
            .WithValue("*");

        star.PartiallyParses("**")
            .LeavingUnparsedInput("*")
            .WithValue("*");

        doubleStar.FailsToParse("a")
            .LeavingUnparsedInput("a")
            .WithMessage("(1, 1): ** expected");

        doubleStar.FailsToParse("*")
            .LeavingUnparsedInput("*")
            .WithMessage("(1, 1): ** expected");

        doubleStar.FailsToParse("* *")
            .LeavingUnparsedInput("* *")
            .WithMessage("(1, 1): ** expected");

        doubleStar.Parses("**")
            .WithValue("**");

        doubleStar.PartiallyParses("***")
            .LeavingUnparsedInput("*")
            .WithValue("**");
    }
}

public class AlternationTests
{
    readonly IParser<string> A, B, C;

    public AlternationTests()
    {
        A = Pattern("A", @"A");
        B = Pattern("B", @"B");
        C = Pattern("C", @"C");
    }

    public void ChoosingBetweenZeroAlternativesAlwaysFails()
    {
        Choice<string>().FailsToParse("ABC").LeavingUnparsedInput("ABC");
    }

    public void ChoosingBetweenOneAlternativeParserIsEquivalentToThatParser()
    {
        Choice(A).Parses("A").WithValue("A");
        Choice(A).PartiallyParses("AB").LeavingUnparsedInput("B").WithValue("A");
        Choice(A).FailsToParse("B").LeavingUnparsedInput("B").WithMessage("(1, 1): A expected");
    }

    public void FirstParserCanSucceedWithoutExecutingOtherAlternatives()
    {
        Choice(A, NeverExecuted).Parses("A").WithValue("A");
    }

    public void SubsequentParserCanSucceedWhenPreviousParsersFailWithoutConsumingInput()
    {
        Choice(B, A).Parses("A").WithValue("A");
        Choice(C, B, A).Parses("A").WithValue("A");
    }

    public void SubsequentParserWillNotBeAttemptedWhenPreviousParserFailsAfterConsumingInput()
    {
        //As soon as something consumes input, it's failure and message win.

        var AB = from a in A
            from b in B
            select a + b;

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

        IParser<string> succeedWithoutConsuming = new LambdaParser<string>(input => new Parsed<string>(null, input));

        var reply = Choice(A, succeedWithoutConsuming).Parses("");
        reply.ErrorMessages.ToString().ShouldBe("A expected");

        reply = Choice(A, B, succeedWithoutConsuming).Parses("");
        reply.ErrorMessages.ToString().ShouldBe("A or B expected");

        reply = Choice(A, succeedWithoutConsuming, B).Parses("");
        reply.ErrorMessages.ToString().ShouldBe("A expected");
    }

    static readonly IParser<string> NeverExecuted = new LambdaParser<string>(input =>
    {
        throw new Exception("Parser 'NeverExecuted' should not have been executed.");
    });
}
