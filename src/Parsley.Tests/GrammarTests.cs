using System.Text.RegularExpressions;
using static Parsley.Grammar;

namespace Parsley.Tests;

class GrammarTests
{
    static readonly Parser<string> Digit = Pattern("Digit", @"[0-9]");
    static readonly Parser<string> Letter = Pattern("Letter", @"[a-zA-Z]");

    readonly Parser<string> A, B, AB, COMMA;

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
        Grammar<string>.Fail.FailsToParse("ABC", "ABC");
    }

    public void CanDetectTheEndOfInputWithoutAdvancing()
    {
        EndOfInput.Parses("").WithValue("");
        EndOfInput.FailsToParse("!", "!").WithMessage("(1, 1): end of input expected");
    }

    public void CanDemandThatAGivenParserRecognizesTheNextConsumableInput()
    {
        Letter.Parses("A").WithValue("A");
        Letter.FailsToParse("0", "0").WithMessage("(1, 1): Letter expected");

        Digit.FailsToParse("A", "A").WithMessage("(1, 1): Digit expected");
        Digit.Parses("0").WithValue("0");
    }

    public void CanDemandThatAGivenTokenLiteralAppearsNext()
    {
        A.Parses("A").WithValue("A");
        A.PartiallyParses("A!", "!").WithValue("A");
        A.FailsToParse("B", "B").WithMessage("(1, 1): A expected");
    }

    public void ApplyingARuleZeroOrMoreTimes()
    {
        var parser = ZeroOrMore(AB);

        parser.Parses("").Value.ShouldBeEmpty();

        parser.PartiallyParses("AB!", "!")
            .WithValue(Literals("AB"));

        parser.PartiallyParses("ABAB!", "!")
            .WithValue(Literals("AB", "AB"));

        parser.FailsToParse("ABABA!", "!")
            .WithMessage("(1, 6): B expected");

        Parser<string> succeedWithoutConsuming = input => new Parsed<string>(null, input, input.Position, input.EndOfInput);
        Action infiniteLoop = () => ZeroOrMore(succeedWithoutConsuming)(new Text(""));

        infiniteLoop
            .ShouldThrow<Exception>()
            .Message.ShouldBe("Parser encountered a potential infinite loop at position (1, 1).");
    }

    public void ApplyingARuleOneOrMoreTimes()
    {
        var parser = OneOrMore(AB);

        parser.FailsToParse("").AtEndOfInput().WithMessage("(1, 1): A expected");

        parser.PartiallyParses("AB!", "!")
            .WithValue(Literals("AB"));

        parser.PartiallyParses("ABAB!", "!")
            .WithValue(Literals("AB", "AB"));

        parser.FailsToParse("ABABA!", "!")
            .WithMessage("(1, 6): B expected");

        Parser<string> succeedWithoutConsuming = input => new Parsed<string>(null, input, input.Position, input.EndOfInput);
        Action infiniteLoop = () => OneOrMore(succeedWithoutConsuming)(new Text(""));

        infiniteLoop
            .ShouldThrow<Exception>()
            .Message.ShouldBe("Parser encountered a potential infinite loop at position (1, 1).");
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

    public void ParsingAnOptionalRuleZeroOrOneTimes()
    {
        Optional(AB).PartiallyParses("AB.", ".").WithValue("AB");
        Optional(AB).PartiallyParses(".", ".").WithValue((string)null);
        Optional(AB).FailsToParse("AC.", "C.").WithMessage("(1, 2): B expected");
    }

    public void AttemptingToParseRuleButBacktrackingUponFailure()
    {
        //When p succeeds, Attempt(p) is the same as p.
        Attempt(AB).Parses("AB").WithValue("AB");

        //When p fails without consuming input, Attempt(p) is the same as p.
        Attempt(AB).FailsToParse("!", "!").WithMessage("(1, 1): A expected");

        //When p fails after consuming input, Attempt(p) backtracks before reporting failure.
        Attempt(AB).FailsToParse("A!", "A!").WithMessage("(1, 1): [(1, 2): B expected]");
    }

    public void ImprovingDefaultMessagesWithAKnownExpectation()
    {
        var labeled = Label(AB, "'A' followed by 'B'");

        //When p succeeds after consuming input, Label(p) is the same as p.
        AB.Parses("AB").WithNoMessage().WithValue("AB");
        labeled.Parses("AB").WithNoMessage().WithValue("AB");

        //When p fails after consuming input, Label(p) is the same as p.
        AB.FailsToParse("A!", "!").WithMessage("(1, 2): B expected");
        labeled.FailsToParse("A!", "!").WithMessage("(1, 2): B expected");

        //When p succeeds but does not consume input, Label(p) still succeeds but the potential error is included.
        var succeedWithoutConsuming = "$".SucceedWithThisValue();
        succeedWithoutConsuming
            .PartiallyParses("!", "!")
            .WithNoMessage()
            .WithValue("$");
        Label(succeedWithoutConsuming, "nothing")
            .PartiallyParses("!", "!")
            .WithMessage("(1, 1): nothing expected")
            .WithValue("$");

        //When p fails but does not consume input, Label(p) fails with the given expectation.
        AB.FailsToParse("!", "!").WithMessage("(1, 1): A expected");
        labeled.FailsToParse("!", "!").WithMessage("(1, 1): 'A' followed by 'B' expected");
    }

    public void ProvidesConveniencePrimitiveForRecognizingNamedRegexPatterns()
    {
        var lower = Pattern("Lowercase", @"[a-z]+");
        var upper = Pattern("Uppercase", @"[A-Z]+");
        var caseInsensitive = Pattern("Case Insensitive", @"[a-z]+", RegexOptions.IgnoreCase);

        lower.FailsToParse("ABCdef", "ABCdef")
            .WithMessage("(1, 1): Lowercase expected");

        upper.FailsToParse("abcDEF", "abcDEF")
            .WithMessage("(1, 1): Uppercase expected");

        caseInsensitive.FailsToParse("!abcDEF", "!abcDEF")
            .WithMessage("(1, 1): Case Insensitive expected");

        lower.PartiallyParses("abcDEF", "DEF")
            .WithValue("abc");

        upper.Parses("DEF")
            .WithValue("DEF");

        caseInsensitive.Parses("abcDEF")
            .WithValue("abcDEF");
    }

    public void ProvidesConveniencePrimitiveForDefiningKeywords()
    {
        var foo = Keyword("foo");

        foo.FailsToParse("bar", "bar")
            .WithMessage("(1, 1): foo expected");

        foo.Parses("foo")
            .WithValue("foo");

        foo.PartiallyParses("foo bar", " bar")
            .WithValue("foo");

        foo.FailsToParse("foobar", "foobar")
            .WithMessage("(1, 1): foo expected");

        var notJustLetters = () => Keyword(" oops ");
        notJustLetters.ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Keywords may only contain letters. (Parameter 'word')");
    }

    public void ProvidesConveniencePrimitiveForDefiningOperators()
    {
        var star = Operator("*");
        var doubleStar = Operator("**");

        star.FailsToParse("a", "a")
            .WithMessage("(1, 1): * expected");

        star.Parses("*")
            .WithValue("*");

        star.PartiallyParses("* *", " *")
            .WithValue("*");

        star.PartiallyParses("**", "*")
            .WithValue("*");

        doubleStar.FailsToParse("a", "a")
            .WithMessage("(1, 1): ** expected");

        doubleStar.FailsToParse("*", "*")
            .WithMessage("(1, 1): ** expected");

        doubleStar.FailsToParse("* *", "* *")
            .WithMessage("(1, 1): ** expected");

        doubleStar.Parses("**")
            .WithValue("**");

        doubleStar.PartiallyParses("***", "*")
            .WithValue("**");
    }
}

public class AlternationTests
{
    readonly Parser<string> A, B, C;

    public AlternationTests()
    {
        A = Pattern("A", @"A");
        B = Pattern("B", @"B");
        C = Pattern("C", @"C");
    }

    public void ChoosingBetweenZeroAlternativesAlwaysFails()
    {
        Choice<string>().FailsToParse("ABC", "ABC");
    }

    public void ChoosingBetweenOneAlternativeParserIsEquivalentToThatParser()
    {
        Choice(A).Parses("A").WithValue("A");
        Choice(A).PartiallyParses("AB", "B").WithValue("A");
        Choice(A).FailsToParse("B", "B").WithMessage("(1, 1): A expected");
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

        Parser<string> succeedWithoutConsuming = input => new Parsed<string>(null, input, input.Position, input.EndOfInput);

        var reply = Choice(A, succeedWithoutConsuming).Parses("");
        reply.ErrorMessages.ToString().ShouldBe("A expected");

        reply = Choice(A, B, succeedWithoutConsuming).Parses("");
        reply.ErrorMessages.ToString().ShouldBe("A or B expected");

        reply = Choice(A, succeedWithoutConsuming, B).Parses("");
        reply.ErrorMessages.ToString().ShouldBe("A expected");
    }

    static readonly Parser<string> NeverExecuted =
        input => throw new Exception("Parser 'NeverExecuted' should not have been executed.");
}
