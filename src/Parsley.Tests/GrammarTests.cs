using static Parsley.Grammar;

namespace Parsley.Tests;

class GrammarTests
{
    static readonly Parser<char> Digit = Character(char.IsDigit, "Digit");
    static readonly Parser<char> Letter = Character(char.IsLetter, "Letter");

    readonly Parser<char> A, B, COMMA;
    readonly Parser<string> AB;

    public GrammarTests()
    {
        A = Character(x => x == 'A', "A");
        B = Character(x => x == 'B', "B");

        AB = from a in A
            from b in B
            select $"{a}{b}";

        COMMA = Character(x => x == ',', "COMMA");
    }

    static Action<IEnumerable<string>> Literals(params string[] expectedLiterals)
        => actualLiterals => actualLiterals.ShouldBe(expectedLiterals);

    public void CanFailWithoutConsumingInput()
    {
        Grammar<string>.Fail.FailsToParse("ABC", "ABC", "(1, 1): Parse error.");
    }

    public void CanDetectTheEndOfInputWithoutAdvancing()
    {
        EndOfInput.Parses("").WithValue("");
        EndOfInput.FailsToParse("!", "!", "(1, 1): end of input expected");
    }

    public void CanDemandThatAGivenParserRecognizesTheNextConsumableInput()
    {
        Letter.Parses("A").WithValue('A');
        Letter.FailsToParse("0", "0", "(1, 1): Letter expected");

        Digit.FailsToParse("A", "A", "(1, 1): Digit expected");
        Digit.Parses("0").WithValue('0');
    }

    public void CanDemandThatAGivenTokenLiteralAppearsNext()
    {
        A.Parses("A").WithValue('A');
        A.PartiallyParses("A!", "!").WithValue('A');
        A.FailsToParse("B", "B", "(1, 1): A expected");
    }

    public void ApplyingARuleZeroOrMoreTimes()
    {
        var parser = ZeroOrMore(AB);

        parser.Parses("").Value.ShouldBeEmpty();

        parser.PartiallyParses("AB!", "!")
            .WithValue(Literals("AB"));

        parser.PartiallyParses("ABAB!", "!")
            .WithValue(Literals("AB", "AB"));

        parser.FailsToParse("ABABA!", "!", "(1, 6): B expected");

        Parser<string> succeedWithoutConsuming = input => new Parsed<string>(null, input.Position);
        Action infiniteLoop = () => ZeroOrMore(succeedWithoutConsuming)(new Text(""));

        infiniteLoop
            .ShouldThrow<Exception>()
            .Message.ShouldBe("Parser encountered a potential infinite loop at position (1, 1).");
    }

    public void ApplyingARuleOneOrMoreTimes()
    {
        var parser = OneOrMore(AB);

        parser.FailsToParse("", "", "(1, 1): A expected");

        parser.PartiallyParses("AB!", "!")
            .WithValue(Literals("AB"));

        parser.PartiallyParses("ABAB!", "!")
            .WithValue(Literals("AB", "AB"));

        parser.FailsToParse("ABABA!", "!", "(1, 6): B expected");

        Parser<string> succeedWithoutConsuming = input => new Parsed<string>(null, input.Position);
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
        parser.FailsToParse("AB,", "", "(1, 4): A expected");
        parser.FailsToParse("AB,A", "", "(1, 5): B expected");
    }

    public void ApplyingARuleOneOrMoreTimesInterspersedByASeparatorRule()
    {
        var parser = OneOrMore(AB, COMMA);

        parser.FailsToParse("", "", "(1, 1): A expected");
        parser.Parses("AB").WithValue(Literals("AB"));
        parser.Parses("AB,AB").WithValue(Literals("AB", "AB"));
        parser.Parses("AB,AB,AB").WithValue(Literals("AB", "AB", "AB"));
        parser.FailsToParse("AB,", "", "(1, 4): A expected");
        parser.FailsToParse("AB,A", "", "(1, 5): B expected");
    }

    public void ParsingAnOptionalRuleZeroOrOneTimes()
    {
        Optional(AB).PartiallyParses("AB.", ".").WithValue("AB");
        Optional(AB).PartiallyParses(".", ".").WithValue((string)null);
        Optional(AB).FailsToParse("AC.", "C.", "(1, 2): B expected");
    }

    public void AttemptingToParseRuleButBacktrackingUponFailure()
    {
        //When p succeeds, Attempt(p) is the same as p.
        Attempt(AB).Parses("AB").WithValue("AB");

        //When p fails without consuming input, Attempt(p) is the same as p.
        Attempt(AB).FailsToParse("!", "!", "(1, 1): A expected");

        //When p fails after consuming input, Attempt(p) backtracks before reporting failure.
        Attempt(AB).FailsToParse("A!", "A!", "(1, 1): [(1, 2): B expected]");
    }

    public void ImprovingDefaultMessagesWithAKnownExpectation()
    {
        var labeled = Label(AB, "'A' followed by 'B'");

        //When p succeeds after consuming input, Label(p) is the same as p.
        AB.Parses("AB").WithNoMessage().WithValue("AB");
        labeled.Parses("AB").WithNoMessage().WithValue("AB");

        //When p fails after consuming input, Label(p) is the same as p.
        AB.FailsToParse("A!", "!", "(1, 2): B expected");
        labeled.FailsToParse("A!", "!", "(1, 2): B expected");

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
        AB.FailsToParse("!", "!", "(1, 1): A expected");
        labeled.FailsToParse("!", "!", "(1, 1): 'A' followed by 'B' expected");
    }

    public void ProvidesConveniencePrimitiveRecognizingOneCharacterSatisfyingSomePredicate()
    {
        var lower = Character(char.IsLower, "Lowercase");
        var upper = Character(char.IsUpper, "Uppercase");
        var caseInsensitive = Character(char.IsLetter, "Case Insensitive");

        lower.FailsToParse("", "", "(1, 1): Lowercase expected");

        lower.FailsToParse("ABCdef", "ABCdef", "(1, 1): Lowercase expected");

        upper.FailsToParse("abcDEF", "abcDEF", "(1, 1): Uppercase expected");

        caseInsensitive.FailsToParse("!abcDEF", "!abcDEF", "(1, 1): Case Insensitive expected");

        lower.PartiallyParses("abcDEF", "bcDEF").WithValue('a');

        upper.PartiallyParses("DEF", "EF").WithValue('D');

        caseInsensitive.PartiallyParses("abcDEF", "bcDEF").WithValue('a');
    }

    public void ProvidesConveniencePrimitiveRecognizingNonemptySequencesOfCharactersSatisfyingSomePredicate()
    {
        var lower = OneOrMore(char.IsLower, "Lowercase");
        var upper = OneOrMore(char.IsUpper, "Uppercase");
        var caseInsensitive = OneOrMore(char.IsLetter, "Case Insensitive");

        lower.FailsToParse("", "", "(1, 1): Lowercase expected");
        
        lower.FailsToParse("ABCdef", "ABCdef", "(1, 1): Lowercase expected");

        upper.FailsToParse("abcDEF", "abcDEF", "(1, 1): Uppercase expected");

        caseInsensitive.FailsToParse("!abcDEF", "!abcDEF", "(1, 1): Case Insensitive expected");

        lower.PartiallyParses("abcDEF", "DEF").WithValue("abc");

        upper.Parses("DEF").WithValue("DEF");

        caseInsensitive.Parses("abcDEF").WithValue("abcDEF");
    }

    public void ProvidesConveniencePrimitiveForDefiningKeywords()
    {
        var foo = Keyword("foo");

        foo.FailsToParse("", "", "(1, 1): foo expected");
        
        foo.FailsToParse("bar", "bar", "(1, 1): foo expected");
        foo.FailsToParse("fo", "fo", "(1, 1): foo expected");

        foo.PartiallyParses("foo ", " ").WithValue("foo");
        foo.Parses("foo").WithValue("foo");

        foo.PartiallyParses("foo bar", " bar").WithValue("foo");

        foo.FailsToParse("foobar", "foobar", "(1, 1): foo expected");

        var notJustLetters = () => Keyword(" oops ");
        notJustLetters.ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Keywords may only contain letters. (Parameter 'word')");
    }

    public void ProvidesConveniencePrimitiveForDefiningOperators()
    {
        var star = Operator("*");
        var doubleStar = Operator("**");

        star.FailsToParse("a", "a", "(1, 1): * expected");

        star.Parses("*")
            .WithValue("*");

        star.PartiallyParses("* *", " *")
            .WithValue("*");

        star.PartiallyParses("**", "*")
            .WithValue("*");

        doubleStar.FailsToParse("a", "a", "(1, 1): ** expected");

        doubleStar.FailsToParse("*", "*", "(1, 1): ** expected");

        doubleStar.FailsToParse("* *", "* *", "(1, 1): ** expected");

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
        A = from c in Character(x => x == 'A', "A") select c.ToString();
        B = from c in Character(x => x == 'B', "B") select c.ToString();
        C = from c in Character(x => x == 'C', "C") select c.ToString();
    }

    public void ChoosingBetweenZeroAlternativesAlwaysFails()
    {
        Choice<string>().FailsToParse("ABC", "ABC", "(1, 1): Parse error.");
    }

    public void ChoosingBetweenOneAlternativeParserIsEquivalentToThatParser()
    {
        Choice(A).Parses("A").WithValue("A");
        Choice(A).PartiallyParses("AB", "B").WithValue("A");
        Choice(A).FailsToParse("B", "B", "(1, 1): A expected");
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
            select $"{a}{b}";

        Choice(AB, NeverExecuted).FailsToParse("A", "", "(1, 2): B expected");
        Choice(from c in C select c.ToString(), AB, NeverExecuted).FailsToParse("A", "", "(1, 2): B expected");
    }

    public void MergesErrorMessagesWhenParsersFailWithoutConsumingInput()
    {
        Choice(A, B).FailsToParse("", "", "(1, 1): A or B expected");
        Choice(A, B, C).FailsToParse("", "", "(1, 1): A, B or C expected");
    }

    public void MergesPotentialErrorMessagesWhenParserSucceedsWithoutConsumingInput()
    {
        //Choice really shouldn't be used with parsers that can succeed without
        //consuming input.  These tests simply describe the behavior under that
        //unusual situation.

        Parser<string> succeedWithoutConsuming = input => new Parsed<string>(null, input.Position);

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
