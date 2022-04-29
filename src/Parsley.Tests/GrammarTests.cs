using System.Diagnostics.CodeAnalysis;
using static Parsley.Grammar;

namespace Parsley.Tests;

class GrammarTests
{
    static readonly Parser<string> Fail = (ref ReadOnlySpan<char> input, ref Index position, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation) =>
    {
        expectation = "unsatisfiable expectation";
        value = null;
        return false;
    };
    static readonly Parser<char> Digit = Single(char.IsDigit, "Digit");
    static readonly Parser<char> Letter = Single(char.IsLetter, "Letter");

    readonly Parser<char> A, B, COMMA;
    readonly Parser<string> AB;

    public GrammarTests()
    {
        A = Single('A');
        B = Single('B');

        AB = from a in A
            from b in B
            select $"{a}{b}";

        COMMA = Single(',');
    }

    public void CanFailWithoutConsumingInput()
    {
        Fail.FailsToParse("ABC", "ABC", "unsatisfiable expectation expected");
    }

    public void CanDetectTheEndOfInputWithoutAdvancing()
    {
        EndOfInput.Parses("").ShouldBe("");
        EndOfInput.FailsToParse("!", "!", "end of input expected");
    }

    public void CanDemandThatAGivenParserRecognizesTheNextConsumableInput()
    {
        Letter.Parses("A").ShouldBe('A');
        Letter.FailsToParse("0", "0", "Letter expected");

        Digit.FailsToParse("A", "A", "Digit expected");
        Digit.Parses("0").ShouldBe('0');
    }

    public void CanDemandThatAGivenTokenLiteralAppearsNext()
    {
        A.Parses("A").ShouldBe('A');
        A.PartiallyParses("A!", "!").ShouldBe('A');
        A.FailsToParse("B", "B", "A expected");
    }

    public void ApplyingARuleZeroOrMoreTimes()
    {
        var parser = ZeroOrMore(AB);

        parser.Parses("").ShouldBeEmpty();

        parser.PartiallyParses("AB!", "!")
            .Single().ShouldBe("AB");

        parser.PartiallyParses("ABAB!", "!")
            .ShouldBe(new[] { "AB", "AB" });

        parser.FailsToParse("ABABA!", "!", "B expected");

        var succeedWithThisValue = "ignored value".SucceedWithThisValue();
        var infiniteLoop = () =>
        {
            ReadOnlySpan<char> input = "";
            Index index = new(0);
            ZeroOrMore(succeedWithThisValue)(ref input, ref index, out _, out _);
        };

        infiniteLoop
            .ShouldThrow<Exception>()
            .Message.ShouldBe("Parser encountered a potential infinite loop at position 0.");
    }

    public void ApplyingARuleOneOrMoreTimes()
    {
        var parser = OneOrMore(AB);

        parser.FailsToParse("", "", "A expected");

        parser.PartiallyParses("AB!", "!")
            .Single().ShouldBe("AB");

        parser.PartiallyParses("ABAB!", "!")
            .ShouldBe(new[] { "AB", "AB" });

        parser.FailsToParse("ABABA!", "!", "B expected");

        var succeedWithoutConsuming = "ignored value".SucceedWithThisValue();
        var infiniteLoop = () =>
        {
            ReadOnlySpan<char> input = "";
            Index index = new(0);
            OneOrMore(succeedWithoutConsuming)(ref input, ref index, out _, out _);
        };

        infiniteLoop
            .ShouldThrow<Exception>()
            .Message.ShouldBe("Parser encountered a potential infinite loop at position 0.");
    }

    public void ApplyingARuleZeroOrMoreTimesInterspersedByASeparatorRule()
    {
        var parser = ZeroOrMore(AB, COMMA);

        parser.Parses("").ShouldBeEmpty();
        parser.Parses("AB").Single().ShouldBe("AB");
        parser.Parses("AB,AB").ShouldBe(new[] { "AB", "AB" });
        parser.Parses("AB,AB,AB").ShouldBe(new[] { "AB", "AB", "AB" });
        parser.FailsToParse("AB,", "", "A expected");
        parser.FailsToParse("AB,A", "", "B expected");
    }

    public void ApplyingARuleOneOrMoreTimesInterspersedByASeparatorRule()
    {
        var parser = OneOrMore(AB, COMMA);

        parser.FailsToParse("", "", "A expected");
        parser.Parses("AB").Single().ShouldBe("AB");
        parser.Parses("AB,AB").ShouldBe(new[] { "AB", "AB" });
        parser.Parses("AB,AB,AB").ShouldBe(new[] { "AB", "AB", "AB" });
        parser.FailsToParse("AB,", "", "A expected");
        parser.FailsToParse("AB,A", "", "B expected");
    }

    public void ParsingAnOptionalRuleZeroOrOneTimes()
    {
        //Reference Type to Nullable Reference Type
        Optional(AB).PartiallyParses("AB.", ".").ShouldBe("AB");
        Optional(AB).PartiallyParses(".", ".").ShouldBe(null);
        Optional(AB).FailsToParse("AC.", "C.", "B expected");

        //Value Type to Nullable Value Type
        Optional(A).PartiallyParses("AB.", "B.").ShouldBe('A');
        Optional(A).PartiallyParses(".", ".").ShouldBe(null);
        Optional(B).PartiallyParses("A", "A").ShouldBe(null);
        Optional(B).Parses("").ShouldBe(null);

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
        AB.Parses("AB").ShouldBe("AB");
        Attempt(AB).Parses("AB").ShouldBe("AB");

        //When p fails without consuming input, Attempt(p) is the same as p.
        AB.FailsToParse("!", "!", "A expected");
        Attempt(AB).FailsToParse("!", "!", "A expected");

        //When p fails after consuming input, Attempt(p) backtracks before reporting failure.
        //This error message is naturally disappointing on its own as it describe an expectation
        //at a deeper location than we stopped at. In practice, this is part of some larger
        //Choice where either another option's message will supersede this one, or the entire
        //Choice fails without consuming input, in which case you're better off applying a Label
        //on the options which will again supersede this message.
        AB.FailsToParse("A!", "!", "B expected");
        Attempt(AB).FailsToParse("A!", "A!", "B expected");
    }

    public void NegatingAnotherParseRuleWithoutConsumingInput()
    {
        //When p succeeds, Not(p) fails.
        //When p fails, Not(p) succeeds.
        //Not(p) never consumes input, even if p fails after consuming input.

        AB.Parses("AB").ShouldBe("AB");
        AB.FailsToParse("A!", "!", "B expected");
        AB.FailsToParse("BA", "BA", "A expected");

        Not(AB).FailsToParse("AB", "AB", "parse failure expected");
        Not(AB).PartiallyParses("A!", "A!").ShouldBe(Void.Value);
        Not(AB).PartiallyParses("BA", "BA").ShouldBe(Void.Value);
    }

    public void ImprovingDefaultMessagesWithAKnownExpectation()
    {
        var labeled = Label(AB, "'A' followed by 'B'");

        //When p succeeds after consuming input, Label(p) is the same as p.
        AB.Parses("AB").ShouldBe("AB");
        labeled.Parses("AB").ShouldBe("AB");

        //When p fails after consuming input, Label(p) is the same as p.
        AB.FailsToParse("A!", "!", "B expected");
        labeled.FailsToParse("A!", "!", "B expected");

        //When p succeeds but does not consume input, Label(p) still succeeds but the potential error is included.
        var succeedWithoutConsuming = "$".SucceedWithThisValue();
        succeedWithoutConsuming
            .PartiallyParses("!", "!")
            .ShouldBe("$");
        Label(succeedWithoutConsuming, "nothing")
            .PartiallyParses("!", "!")
            .ShouldBe("$");

        //When p fails but does not consume input, Label(p) fails with the given expectation.
        AB.FailsToParse("!", "!", "A expected");
        labeled.FailsToParse("!", "!", "'A' followed by 'B' expected");
    }

    public void ProvidesBacktrackingTraceUponExtremeFailureOfLookaheadParsers()
    {
        var sequence = (char first, char second) =>
            from char1 in Single(first)
            from char2 in Single(second)
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

            //Note intermediate expectations are preserved, but describe
            //some deeper location in the input than the current position.
            //The order indicates the order that the problems were handled.
            //It is recommended that such extreme usages of Attempt be supplemented
            //with Label in order to better describe the error.

            "(B, (C or D), or E) expected");

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

            //Note intermediate expectations are preserved, but describe
            //some deeper location in the input than the current position.
            //The order indicates the order that the problems were handled.
            //It is recommended that such extreme usages of Attempt be supplemented
            //with Label in order to better describe the error.

            "(B, (C or D), or E) expected");

        Choice( //Phase out that irrelevant outermost Attempt...
            Attempt(ab),
            Label(Choice( //... instead labeling the nested pattern in an attempt to improve error messages...
                Attempt(ac),
                Attempt(ad)
            ), "A[C|D]"),
            Attempt(ae)
        ).FailsToParse("AF", "AF", //... demonstrating how the intermediate error information is affected.

            //Note the first and third intermediate expectations are preserved, but
            //describe some deeper location in the input than the current position.
            //Note that the second expectation is improved, appropriate to the current
            //position. The order indicates the order that the problems were handled.
            //It is recommended that such extreme usages of Attempt be supplemented
            //with more Label calls in order to better describe the error.

            "(B, A[C|D], or E) expected");

        Choice(
            //Label all of the backtracking choices in an attempt to improve error messages...
            Label(Attempt(ab), "AB"),
            Label(Choice(
                Attempt(ac),
                Attempt(ad)
            ), "A[C|D]"),
            Label(Attempt(ae), "AE")
        ).FailsToParse("AF", "AF", //... demonstrating how the error message once again respects the overall location.

            //Note how the full error message now respects the current position.

            "(AB, A[C|D], or AE) expected");
    }

    public void ProvidesConveniencePrimitiveRecognizingSingleExpectedNextItem()
    {
        var x = Single('x');

        x.FailsToParse("", "", "x expected");
        x.FailsToParse("yz", "yz", "x expected");
        x.PartiallyParses("xyz", "yz").ShouldBe('x');
    }

    public void ProvidesConveniencePrimitiveRecognizingSingleNextItemSatisfyingSomePredicate()
    {
        var lower = Single(char.IsLower, "Lowercase");
        var upper = Single(char.IsUpper, "Uppercase");
        var caseInsensitive = Single(char.IsLetter, "Case Insensitive");

        lower.FailsToParse("", "", "Lowercase expected");

        lower.FailsToParse("ABCdef", "ABCdef", "Lowercase expected");

        upper.FailsToParse("abcDEF", "abcDEF", "Uppercase expected");

        caseInsensitive.FailsToParse("!abcDEF", "!abcDEF", "Case Insensitive expected");

        lower.PartiallyParses("abcDEF", "bcDEF").ShouldBe('a');

        upper.PartiallyParses("DEF", "EF").ShouldBe('D');

        caseInsensitive.PartiallyParses("abcDEF", "bcDEF").ShouldBe('a');
    }

    public void ProvidesConveniencePrimitiveRecognizingOptionalSequencesOfItemsSatisfyingSomePredicate()
    {
        var lower = ZeroOrMore(char.IsLower);
        var upper = ZeroOrMore(char.IsUpper);
        var caseInsensitive = ZeroOrMore(char.IsLetter);

        lower.Parses("").ShouldBe("");

        lower.PartiallyParses("ABCdef", "ABCdef").ShouldBe("");

        upper.PartiallyParses("abcDEF", "abcDEF").ShouldBe("");

        caseInsensitive.PartiallyParses("!abcDEF", "!abcDEF").ShouldBe("");

        lower.PartiallyParses("abcDEF", "DEF").ShouldBe("abc");

        upper.Parses("DEF").ShouldBe("DEF");

        caseInsensitive.Parses("abcDEF").ShouldBe("abcDEF");
    }

    public void ProvidesConveniencePrimitiveRecognizingNonemptySequencesOfItemsSatisfyingSomePredicate()
    {
        var lower = OneOrMore(char.IsLower, "Lowercase");
        var upper = OneOrMore(char.IsUpper, "Uppercase");
        var caseInsensitive = OneOrMore(char.IsLetter, "Case Insensitive");

        lower.FailsToParse("", "", "Lowercase expected");
        
        lower.FailsToParse("ABCdef", "ABCdef", "Lowercase expected");

        upper.FailsToParse("abcDEF", "abcDEF", "Uppercase expected");

        caseInsensitive.FailsToParse("!abcDEF", "!abcDEF", "Case Insensitive expected");

        lower.PartiallyParses("abcDEF", "DEF").ShouldBe("abc");

        upper.Parses("DEF").ShouldBe("DEF");

        caseInsensitive.Parses("abcDEF").ShouldBe("abcDEF");
    }

    public void ProvidesConveniencePrimitiveForDefiningKeywords()
    {
        var foo = Keyword("foo");

        foo.FailsToParse("", "", "foo expected");
        
        foo.FailsToParse("bar", "bar", "foo expected");
        foo.FailsToParse("fo", "fo", "foo expected");

        foo.PartiallyParses("foo ", " ").ShouldBe("foo");
        foo.Parses("foo").ShouldBe("foo");

        foo.PartiallyParses("foo bar", " bar").ShouldBe("foo");

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
            .ShouldBe("*");

        star.PartiallyParses("* *", " *")
            .ShouldBe("*");

        star.PartiallyParses("**", "*")
            .ShouldBe("*");

        doubleStar.FailsToParse("a", "a", "** expected");

        doubleStar.FailsToParse("*", "*", "** expected");

        doubleStar.FailsToParse("* *", "* *", "** expected");

        doubleStar.Parses("**")
            .ShouldBe("**");

        doubleStar.PartiallyParses("***", "*")
            .ShouldBe("**");
    }
}

public class AlternationTests
{
    readonly Parser<string> A, B, C;

    public AlternationTests()
    {
        A = from c in Single('A') select c.ToString();
        B = from c in Single('B') select c.ToString();
        C = from c in Single('C') select c.ToString();
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
        Choice(A, NeverExecuted).Parses("A").ShouldBe("A");
    }

    public void SubsequentParserCanSucceedWhenPreviousParsersFailWithoutConsumingInput()
    {
        Choice(B, A).Parses("A").ShouldBe("A");
        Choice(C, B, A).Parses("A").ShouldBe("A");
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

        Parser<string> succeedWithoutConsuming = (ref ReadOnlySpan<char> input, ref Index position, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation) =>
        {
            expectation = null;
            value = "atypical value";
            return true;
        };

        Choice(A, succeedWithoutConsuming).Parses("").ShouldBe("atypical value");
        Choice(A, B, succeedWithoutConsuming).Parses("").ShouldBe("atypical value");
        Choice(A, succeedWithoutConsuming, B).Parses("").ShouldBe("atypical value");
    }

    static readonly Parser<string> NeverExecuted =
        (ref ReadOnlySpan<char> input, ref Index position, [NotNullWhen(true)] out string? value, [NotNullWhen(false)] out string? expectation)
            => throw new Exception("Parser 'NeverExecuted' should not have been executed.");
}
