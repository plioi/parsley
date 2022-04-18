using static Parsley.Grammar;

namespace Parsley.Tests;

class GrammarTests
{
    static readonly Parser<string> Fail = (ref Text input) => new Error<string>("unsatisfiable expectation");
    static readonly Parser<char> Digit = Character(char.IsDigit, "Digit");
    static readonly Parser<char> Letter = Character(char.IsLetter, "Letter");

    readonly Parser<char> A, B, COMMA;
    readonly Parser<string> AB;

    public GrammarTests()
    {
        A = Character('A');
        B = Character('B');

        AB = from a in A
            from b in B
            select $"{a}{b}";

        COMMA = Character(',');
    }

    public void CanFailWithoutConsumingInput()
    {
        Fail.FailsToParse("ABC", "ABC", "unsatisfiable expectation expected");
    }

    public void CanDetectTheEndOfInputWithoutAdvancing()
    {
        EndOfInput.Parses("").Value.ShouldBe("");
        EndOfInput.FailsToParse("!", "!", "end of input expected");
    }

    public void CanDemandThatAGivenParserRecognizesTheNextConsumableInput()
    {
        Letter.Parses("A").Value.ShouldBe('A');
        Letter.FailsToParse("0", "0", "Letter expected");

        Digit.FailsToParse("A", "A", "Digit expected");
        Digit.Parses("0").Value.ShouldBe('0');
    }

    public void CanDemandThatAGivenTokenLiteralAppearsNext()
    {
        A.Parses("A").Value.ShouldBe('A');
        A.PartiallyParses("A!", "!").Value.ShouldBe('A');
        A.FailsToParse("B", "B", "A expected");
    }

    public void ApplyingARuleZeroOrMoreTimes()
    {
        var parser = ZeroOrMore(AB);

        parser.Parses("").Value.ShouldBeEmpty();

        parser.PartiallyParses("AB!", "!")
            .Value.Single().ShouldBe("AB");

        parser.PartiallyParses("ABAB!", "!")
            .Value.ShouldBe(new[] { "AB", "AB" });

        parser.FailsToParse("ABABA!", "!", "B expected");

        Parser<string> succeedWithoutConsuming = (ref Text input) => new Parsed<string>("ignored value");
        Action infiniteLoop = () =>
        {
            var input = new Text("");
            ZeroOrMore(succeedWithoutConsuming)(ref input);
        };

        infiniteLoop
            .ShouldThrow<Exception>()
            .Message.ShouldBe("Parser encountered a potential infinite loop at position (1, 1).");
    }

    public void ApplyingARuleOneOrMoreTimes()
    {
        var parser = OneOrMore(AB);

        parser.FailsToParse("", "", "A expected");

        parser.PartiallyParses("AB!", "!")
            .Value.Single().ShouldBe("AB");

        parser.PartiallyParses("ABAB!", "!")
            .Value.ShouldBe(new[] { "AB", "AB" });

        parser.FailsToParse("ABABA!", "!", "B expected");

        Parser<string> succeedWithoutConsuming = (ref Text input) => new Parsed<string>("ignored value");
        Action infiniteLoop = () =>
        {
            var input = new Text("");
            OneOrMore(succeedWithoutConsuming)(ref input);
        };

        infiniteLoop
            .ShouldThrow<Exception>()
            .Message.ShouldBe("Parser encountered a potential infinite loop at position (1, 1).");
    }

    public void ApplyingARuleZeroOrMoreTimesInterspersedByASeparatorRule()
    {
        var parser = ZeroOrMore(AB, COMMA);

        parser.Parses("").Value.ShouldBeEmpty();
        parser.Parses("AB").Value.Single().ShouldBe("AB");
        parser.Parses("AB,AB").Value.ShouldBe(new[] { "AB", "AB" });
        parser.Parses("AB,AB,AB").Value.ShouldBe(new[] { "AB", "AB", "AB" });
        parser.FailsToParse("AB,", "", "A expected");
        parser.FailsToParse("AB,A", "", "B expected");
    }

    public void ApplyingARuleOneOrMoreTimesInterspersedByASeparatorRule()
    {
        var parser = OneOrMore(AB, COMMA);

        parser.FailsToParse("", "", "A expected");
        parser.Parses("AB").Value.Single().ShouldBe("AB");
        parser.Parses("AB,AB").Value.ShouldBe(new[] { "AB", "AB" });
        parser.Parses("AB,AB,AB").Value.ShouldBe(new[] { "AB", "AB", "AB" });
        parser.FailsToParse("AB,", "", "A expected");
        parser.FailsToParse("AB,A", "", "B expected");
    }

    public void ParsingAnOptionalRuleZeroOrOneTimes()
    {
        //Reference Type to Nullable Reference Type
        Optional(AB).PartiallyParses("AB.", ".").Value.ShouldBe("AB");
        Optional(AB).PartiallyParses(".", ".").Value.ShouldBe(null);
        Optional(AB).FailsToParse("AC.", "C.", "B expected");

        //Value Type to Nullable Value Type
        Optional(A).PartiallyParses("AB.", "B.").Value.ShouldBe('A');
        Optional(A).PartiallyParses(".", ".").Value.ShouldBe(null);
        Optional(B).PartiallyParses("A", "A").Value.ShouldBe(null);
        Optional(B).Parses("").Value.ShouldBe(null);

        //Alternate possibilities are not supported when nullable
        //reference types are enabled:
        //
        //  Nullable Reference Type to Nullable Reference Type
        //  Nullable Value Type to Nullable Value Type
        //
        //These are not supported because these use cases arise
        //from the construction Optional(Optional(...)), which is
        //suspect to begin with.
    }

    public void AttemptingToParseRuleButBacktrackingUponFailure()
    {
        //When p succeeds, Attempt(p) is the same as p.
        AB.Parses("AB").Value.ShouldBe("AB");
        Attempt(AB).Parses("AB").Value.ShouldBe("AB");

        //When p fails without consuming input, Attempt(p) is the same as p.
        AB.FailsToParse("!", "!", "A expected");
        Attempt(AB).FailsToParse("!", "!", "A expected");

        //When p fails after consuming input, Attempt(p) backtracks before reporting failure.
        AB.FailsToParse("A!", "!", "B expected");
        Attempt(AB).FailsToParse("A!", "A!", "B at (1, 2) expected");
    }

    public void ImprovingDefaultMessagesWithAKnownExpectation()
    {
        var labeled = Label(AB, "'A' followed by 'B'");

        //When p succeeds after consuming input, Label(p) is the same as p.
        AB.Parses("AB").Value.ShouldBe("AB");
        labeled.Parses("AB").Value.ShouldBe("AB");

        //When p fails after consuming input, Label(p) is the same as p.
        AB.FailsToParse("A!", "!", "B expected");
        labeled.FailsToParse("A!", "!", "B expected");

        //When p succeeds but does not consume input, Label(p) still succeeds but the potential error is included.
        var succeedWithoutConsuming = "$".SucceedWithThisValue();
        succeedWithoutConsuming
            .PartiallyParses("!", "!")
            .Value.ShouldBe("$");
        Label(succeedWithoutConsuming, "nothing")
            .PartiallyParses("!", "!")
            .Value.ShouldBe("$");

        //When p fails but does not consume input, Label(p) fails with the given expectation.
        AB.FailsToParse("!", "!", "A expected");
        labeled.FailsToParse("!", "!", "'A' followed by 'B' expected");
    }

    public void ProvidesBacktrackingTraceUponExtremeFailureOfLookaheadParsers()
    {
        var sequence = (char first, char second) =>
            from char1 in Character(first)
            from char2 in Character(second)
            select $"{char1}{char2}";

        var ab = sequence('A', 'B');
        var ac = sequence('A', 'C');
        var ad = sequence('A', 'D');
        var ae = sequence('A', 'E');

        Choice(
                ab, //Since we fail here while consuming a, we fail here.
                Choice(
                    ac,
                    ad
                ),
                ae
            ).FailsToParse("AE", "E", "B expected");

        Choice(
            Attempt(ab), //Allow rewinding the consumption of a...
            Choice(
                ac, //...arriving at the failure to find c.
                ad
            ),
            ae
        ).FailsToParse("AE", "E", "C expected");

        Choice(
            Attempt(ab),
            Choice(
                Attempt(ac), //Allow rewinding the consumption of c...
                ad //...arriving at the failure to find d.
            ),
            ae
        ).FailsToParse("AE", "E", "D expected");

        Choice(
            Attempt(ab),
            Choice(
                Attempt(ac),
                Attempt(ad) //Allow rewinding the consumption of d...
            ),
            ae //...arriving at the success of finding e.
        ).Parses("AE"); //Note intermediate error information is discarded by this point.

        // Now try that again but for input that will still fail...

        Choice(
            Attempt(ab),
            Choice(
                Attempt(ac),
                Attempt(ad) //Allow rewinding the consumption of d...
            ),
            ae //...arriving at the failure to find e.
        ).FailsToParse("AF", "F", "E expected");

        Choice(
            Attempt(ab),
            Choice(
                Attempt(ac),
                Attempt(ad)
            ),
            Attempt(ae) //Allow rewinding the consumption of e (and a since all a-consuming choices allow rewind)...
        ).FailsToParse("AF", "AF", //...arriving at the failure to find f, having consumed nothing.

            //Note intermediate error information is preserved, to guide
            //troubleshooting catastrophic failure. The order indicates
            //the order that the problems were handled.

            "(B at (1, 2), (C at (1, 2) or D at (1, 2)), or E at (1, 2)) expected");

        Attempt( //Excessively attempt the non-consuming choice itself...
            Choice(
                Attempt(ab),
                Choice(
                    Attempt(ac),
                    Attempt(ad)
                ),
                Attempt(ae)
            )
        ).FailsToParse("AF", "AF", //... demonstrating the intermediate error information is NOT discarded.

            //Note intermediate error information is preserved, to guide
            //troubleshooting catastrophic failure. The order indicates
            //the order that the problems were handled.

            "(B at (1, 2), (C at (1, 2) or D at (1, 2)), or E at (1, 2)) expected");

        Choice( //Phase out that irrelevant outermost Attempt...
            Attempt(ab),
            Label(Choice( //... instead labeling the nested pattern in an attempt to improve error messages...
                Attempt(ac),
                Attempt(ad)
            ), "A[C|D]"),
            Attempt(ae)
        ).FailsToParse("AF", "AF", //... demonstrating how the intermediate error information is affected.

            //Note intermediate error information is preserved, to guide
            //troubleshooting catastrophic failure, but that the Label
            //is respected to simplify that segment.

            "(B at (1, 2), A[C|D], or E at (1, 2)) expected");
    }

    public void ProvidesConveniencePrimitiveRecognizingOneExpectedCharacter()
    {
        var x = Character('x');

        x.FailsToParse("", "", "x expected");
        x.FailsToParse("yz", "yz", "x expected");
        x.PartiallyParses("xyz", "yz").Value.ShouldBe('x');
    }

    public void ProvidesConveniencePrimitiveRecognizingOneCharacterSatisfyingSomePredicate()
    {
        var lower = Character(char.IsLower, "Lowercase");
        var upper = Character(char.IsUpper, "Uppercase");
        var caseInsensitive = Character(char.IsLetter, "Case Insensitive");

        lower.FailsToParse("", "", "Lowercase expected");

        lower.FailsToParse("ABCdef", "ABCdef", "Lowercase expected");

        upper.FailsToParse("abcDEF", "abcDEF", "Uppercase expected");

        caseInsensitive.FailsToParse("!abcDEF", "!abcDEF", "Case Insensitive expected");

        lower.PartiallyParses("abcDEF", "bcDEF").Value.ShouldBe('a');

        upper.PartiallyParses("DEF", "EF").Value.ShouldBe('D');

        caseInsensitive.PartiallyParses("abcDEF", "bcDEF").Value.ShouldBe('a');
    }

    public void ProvidesConveniencePrimitiveRecognizingOptionalSequencesOfCharactersSatisfyingSomePredicate()
    {
        var lower = ZeroOrMore(char.IsLower);
        var upper = ZeroOrMore(char.IsUpper);
        var caseInsensitive = ZeroOrMore(char.IsLetter);

        lower.Parses("").Value.ShouldBe("");

        lower.PartiallyParses("ABCdef", "ABCdef").Value.ShouldBe("");

        upper.PartiallyParses("abcDEF", "abcDEF").Value.ShouldBe("");

        caseInsensitive.PartiallyParses("!abcDEF", "!abcDEF").Value.ShouldBe("");

        lower.PartiallyParses("abcDEF", "DEF").Value.ShouldBe("abc");

        upper.Parses("DEF").Value.ShouldBe("DEF");

        caseInsensitive.Parses("abcDEF").Value.ShouldBe("abcDEF");
    }

    public void ProvidesConveniencePrimitiveRecognizingNonemptySequencesOfCharactersSatisfyingSomePredicate()
    {
        var lower = OneOrMore(char.IsLower, "Lowercase");
        var upper = OneOrMore(char.IsUpper, "Uppercase");
        var caseInsensitive = OneOrMore(char.IsLetter, "Case Insensitive");

        lower.FailsToParse("", "", "Lowercase expected");
        
        lower.FailsToParse("ABCdef", "ABCdef", "Lowercase expected");

        upper.FailsToParse("abcDEF", "abcDEF", "Uppercase expected");

        caseInsensitive.FailsToParse("!abcDEF", "!abcDEF", "Case Insensitive expected");

        lower.PartiallyParses("abcDEF", "DEF").Value.ShouldBe("abc");

        upper.Parses("DEF").Value.ShouldBe("DEF");

        caseInsensitive.Parses("abcDEF").Value.ShouldBe("abcDEF");
    }

    public void ProvidesConveniencePrimitiveForDefiningKeywords()
    {
        var foo = Keyword("foo");

        foo.FailsToParse("", "", "foo expected");
        
        foo.FailsToParse("bar", "bar", "foo expected");
        foo.FailsToParse("fo", "fo", "foo expected");

        foo.PartiallyParses("foo ", " ").Value.ShouldBe("foo");
        foo.Parses("foo").Value.ShouldBe("foo");

        foo.PartiallyParses("foo bar", " bar").Value.ShouldBe("foo");

        foo.FailsToParse("foobar", "foobar", "foo expected");

        var notJustLetters = () => Keyword(" oops ");
        notJustLetters.ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Keywords may only contain letters. (Parameter 'word')");
    }

    public void ProvidesConveniencePrimitiveForDefiningOperators()
    {
        var star = Operator("*");
        var doubleStar = Operator("**");

        star.FailsToParse("a", "a", "* expected");

        star.Parses("*")
            .Value.ShouldBe("*");

        star.PartiallyParses("* *", " *")
            .Value.ShouldBe("*");

        star.PartiallyParses("**", "*")
            .Value.ShouldBe("*");

        doubleStar.FailsToParse("a", "a", "** expected");

        doubleStar.FailsToParse("*", "*", "** expected");

        doubleStar.FailsToParse("* *", "* *", "** expected");

        doubleStar.Parses("**")
            .Value.ShouldBe("**");

        doubleStar.PartiallyParses("***", "*")
            .Value.ShouldBe("**");
    }
}

public class AlternationTests
{
    readonly Parser<string> A, B, C;

    public AlternationTests()
    {
        A = from c in Character('A') select c.ToString();
        B = from c in Character('B') select c.ToString();
        C = from c in Character('C') select c.ToString();
    }

    public void ChoosingRequiresAtLeastTwoParsersToChooseBetween()
    {
        var attemptChoiceBetweenZeroAlternatives = () => Choice<string>();
        attemptChoiceBetweenZeroAlternatives
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Choice requires at least two parsers to choose between. (Parameter 'parsers')");

        var attemptChoiceBetweenOneAlternatives = () => Choice(A);
        attemptChoiceBetweenOneAlternatives
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Choice requires at least two parsers to choose between. (Parameter 'parsers')");
    }

    public void FirstParserCanSucceedWithoutExecutingOtherAlternatives()
    {
        Choice(A, NeverExecuted).Parses("A").Value.ShouldBe("A");
    }

    public void SubsequentParserCanSucceedWhenPreviousParsersFailWithoutConsumingInput()
    {
        Choice(B, A).Parses("A").Value.ShouldBe("A");
        Choice(C, B, A).Parses("A").Value.ShouldBe("A");
    }

    public void SubsequentParserWillNotBeAttemptedWhenPreviousParserFailsAfterConsumingInput()
    {
        //As soon as something consumes input, it's failure and message win.

        var AB = from a in A
            from b in B
            select $"{a}{b}";

        Choice(AB, NeverExecuted).FailsToParse("A", "", "B expected");
        Choice(C, AB, NeverExecuted).FailsToParse("A", "", "B expected");
    }

    public void MergesErrorMessagesWhenParsersFailWithoutConsumingInput()
    {
        Choice(A, B).FailsToParse("", "", "(A or B) expected");
        Choice(A, B, C).FailsToParse("", "", "(A, B, or C) expected");
    }

    public void MergesPotentialErrorMessagesWhenParserSucceedsWithoutConsumingInput()
    {
        //Choice really shouldn't be used with parsers that can succeed without
        //consuming input. These tests simply describe the behavior under that
        //unusual situation.

        Parser<string> succeedWithoutConsuming = (ref Text input) => new Parsed<string>("atypical value");

        Choice(A, succeedWithoutConsuming).Parses("").Value.ShouldBe("atypical value");
        Choice(A, B, succeedWithoutConsuming).Parses("").Value.ShouldBe("atypical value");
        Choice(A, succeedWithoutConsuming, B).Parses("").Value.ShouldBe("atypical value");
    }

    static readonly Parser<string> NeverExecuted =
        (ref Text input) => throw new Exception("Parser 'NeverExecuted' should not have been executed.");
}
