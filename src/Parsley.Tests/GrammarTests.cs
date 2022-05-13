using static Parsley.Grammar;
using static Parsley.Characters;

namespace Parsley.Tests;

class GrammarTests
{
    static readonly Parser<char, string> Fail = (ReadOnlySpan<char> input, ref int index, out bool succeeded, out string? expectation) =>
    {
        expectation = "unsatisfiable expectation";
        succeeded = false;
        return null;
    };
    static readonly Parser<char, char> Digit = Single(IsDigit, "Digit");
    static readonly Parser<char, char> Letter = Single(IsLetter, "Letter");

    readonly Parser<char, char> A, B;
    readonly Parser<char, string> AB, AND;

    public GrammarTests()
    {
        A = Single('A');
        B = Single('B');

        AB = from a in A
            from b in B
            select $"{a}{b}";

        var ampersand = Single('&');

        AND = from first in ampersand
            from second in ampersand
            select "&&";
    }

    public void CanFailWithoutConsumingInput()
    {
        Fail.FailsToParse("ABC", "ABC", "unsatisfiable expectation expected");
    }

    public void CanDetectTheEndOfInputWithoutAdvancing()
    {
        EndOfInput<char>().Parses("").ShouldBe(Void.Value);
        EndOfInput<char>().FailsToParse("!", "!", "end of input expected");
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

        var succeedWithThisValue = "ignored value".SucceedWithThisValue<char, string>();
        var infiniteLoop = () =>
        {
            ReadOnlySpan<char> input = "";
            int index = 0;
            ZeroOrMore(succeedWithThisValue)(input, ref index, out _, out _);
        };

        infiniteLoop
            .ShouldThrow<Exception>()
            .Message.ShouldBe("Parser encountered a potential infinite loop at index 0.");
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

        var succeedWithoutConsuming = "ignored value".SucceedWithThisValue<char, string>();
        var infiniteLoop = () =>
        {
            ReadOnlySpan<char> input = "";
            int index = 0;
            OneOrMore(succeedWithoutConsuming)(input, ref index, out _, out _);
        };

        infiniteLoop
            .ShouldThrow<Exception>()
            .Message.ShouldBe("Parser encountered a potential infinite loop at index 0.");
    }

    public void ApplyingARuleZeroOrMoreTimesInterspersedByASeparatorRule()
    {
        var parser = ZeroOrMore(AB, AND);

        parser.Parses("").ShouldBeEmpty();
        parser.PartiallyParses("&", "&").ShouldBeEmpty();

        parser.FailsToParse("A", "", "B expected");
        parser.FailsToParse("A&", "&", "B expected");
        parser.Parses("AB").Single().ShouldBe("AB");
        parser.FailsToParse("AB&", "", "& expected");
        parser.FailsToParse("AB&&", "", "A expected");
        parser.FailsToParse("AB&&A", "", "B expected");
        parser.Parses("AB&&AB").ShouldBe(new[] { "AB", "AB" });
        parser.Parses("AB&&AB&&AB").ShouldBe(new[] { "AB", "AB", "AB" });
        parser.PartiallyParses("AB&&AB&&ABA", "A").ShouldBe(new[] { "AB", "AB", "AB" });
    }

    public void ApplyingARuleOneOrMoreTimesInterspersedByASeparatorRule()
    {
        var parser = OneOrMore(AB, AND);

        parser.FailsToParse("", "", "A expected");
        parser.FailsToParse("&", "&", "A expected");

        parser.FailsToParse("A", "", "B expected");
        parser.FailsToParse("A&", "&", "B expected");
        parser.Parses("AB").Single().ShouldBe("AB");
        parser.FailsToParse("AB&", "", "& expected");
        parser.FailsToParse("AB&&", "", "A expected");
        parser.FailsToParse("AB&&A", "", "B expected");
        parser.Parses("AB&&AB").ShouldBe(new[] { "AB", "AB" });
        parser.Parses("AB&&AB&&AB").ShouldBe(new[] { "AB", "AB", "AB" });
        parser.PartiallyParses("AB&&AB&&ABA", "A").ShouldBe(new[] { "AB", "AB", "AB" });
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
        var succeedWithoutConsuming = "$".SucceedWithThisValue<char, string>();
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
            //some deeper location in the input than the current index.
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
            //some deeper location in the input than the current index.
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
            //describe some deeper location in the input than the current index.
            //Note that the second expectation is improved, appropriate to the current
            //index. The order indicates the order that the problems were handled.
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

            //Note how the full error message now respects the current index.

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
        var lower = Single(IsLower, "Lowercase");
        var upper = Single(IsUpper, "Uppercase");
        var caseInsensitive = Single(IsLetter, "Case Insensitive");

        lower.FailsToParse("", "", "Lowercase expected");

        lower.FailsToParse("ABCdef", "ABCdef", "Lowercase expected");

        upper.FailsToParse("abcDEF", "abcDEF", "Uppercase expected");

        caseInsensitive.FailsToParse("!abcDEF", "!abcDEF", "Case Insensitive expected");

        lower.PartiallyParses("abcDEF", "bcDEF").ShouldBe('a');

        upper.PartiallyParses("DEF", "EF").ShouldBe('D');

        caseInsensitive.PartiallyParses("abcDEF", "bcDEF").ShouldBe('a');
    }

    public void ProvidesConveniencePrimitiveSkippingOptionalSequencesOfItemsSatisfyingSomePredicate()
    {
        var lower = Skip(IsLower);
        var upper = Skip(IsUpper);
        var caseInsensitive = Skip(IsLetter);

        lower.Parses("").ShouldBe(Void.Value);

        lower.PartiallyParses("ABCdef", "ABCdef").ShouldBe(Void.Value);

        upper.PartiallyParses("abcDEF", "abcDEF").ShouldBe(Void.Value);

        caseInsensitive.PartiallyParses("!abcDEF", "!abcDEF").ShouldBe(Void.Value);

        lower.PartiallyParses("abcDEF", "DEF").ShouldBe(Void.Value);

        upper.Parses("DEF").ShouldBe(Void.Value);

        caseInsensitive.Parses("abcDEF").ShouldBe(Void.Value);
    }

    public void ProvidesConveniencePrimitiveRecognizingOptionalSequencesOfItemsSatisfyingSomePredicate()
    {
        var lower = ZeroOrMore(IsLower);
        var upper = ZeroOrMore(IsUpper);
        var caseInsensitive = ZeroOrMore(IsLetter);

        lower.Parses("").ShouldBe("");

        lower.PartiallyParses("ABCdef", "ABCdef").ShouldBe("");

        upper.PartiallyParses("abcDEF", "abcDEF").ShouldBe("");

        caseInsensitive.PartiallyParses("!abcDEF", "!abcDEF").ShouldBe("");

        lower.PartiallyParses("abcDEF", "DEF").ShouldBe("abc");

        upper.Parses("DEF").ShouldBe("DEF");

        caseInsensitive.Parses("abcDEF").ShouldBe("abcDEF");

        var even = ZeroOrMore<int>(x => x % 2 == 0);
        var empty = Array.Empty<int>();
        even.Parses(empty).ShouldBe(empty);
        even.PartiallyParses(new[] { 1, 2, 4, 6 }, new[] { 1, 2, 4, 6 }).ShouldBe(empty);
        even.PartiallyParses(new[] { 2, 4, 6, 1, 3, 5 }, new[] { 1, 3, 5 }).ShouldBe(new[] { 2, 4, 6 });
        even.Parses(new[] { 2, 4, 6 }).ShouldBe(new[] { 2, 4, 6 });
    }

    public void ProvidesConveniencePrimitiveRecognizingNonemptySequencesOfItemsSatisfyingSomePredicate()
    {
        var lower = OneOrMore(IsLower, "Lowercase");
        var upper = OneOrMore(IsUpper, "Uppercase");
        var caseInsensitive = OneOrMore(IsLetter, "Case Insensitive");

        lower.FailsToParse("", "", "Lowercase expected");
        
        lower.FailsToParse("ABCdef", "ABCdef", "Lowercase expected");

        upper.FailsToParse("abcDEF", "abcDEF", "Uppercase expected");

        caseInsensitive.FailsToParse("!abcDEF", "!abcDEF", "Case Insensitive expected");

        lower.PartiallyParses("abcDEF", "DEF").ShouldBe("abc");

        upper.Parses("DEF").ShouldBe("DEF");

        caseInsensitive.Parses("abcDEF").ShouldBe("abcDEF");

        var even = OneOrMore<int>(x => x % 2 == 0, "even number");
        var empty = Array.Empty<int>();
        even.FailsToParse(empty, empty, "even number expected");
        even.FailsToParse(new[] { 1, 2, 4, 6 }, new[] { 1, 2, 4, 6 }, "even number expected");
        even.PartiallyParses(new[] { 2, 4, 6, 1, 3, 5 }, new[] { 1, 3, 5 }).ShouldBe(new[] { 2, 4, 6 });
        even.Parses(new[] { 2, 4, 6 }).ShouldBe(new[] { 2, 4, 6 });
    }

    public void ProvidesConveniencePrimitiveRecognizingSequencesOfItemsSatisfyingSomePredicateAFixedNumberOfTimes()
    {
        var lower2 = Repeat(IsLower, 2, "2 Lowercase");
        var lower3 = Repeat(IsLower, 3, "3 Lowercase");
        var lower4 = Repeat(IsLower, 4, "4 Lowercase");

        lower2.FailsToParse("", "", "2 Lowercase expected");
        lower3.FailsToParse("", "", "3 Lowercase expected");
        lower4.FailsToParse("", "", "4 Lowercase expected");

        lower2.FailsToParse("ABCdef", "ABCdef", "2 Lowercase expected");
        lower3.FailsToParse("ABCdef", "ABCdef", "3 Lowercase expected");
        lower4.FailsToParse("ABCdef", "ABCdef", "4 Lowercase expected");

        lower2.PartiallyParses("abcDEF", "cDEF").ShouldBe("ab");
        lower3.PartiallyParses("abcDEF", "DEF").ShouldBe("abc");
        lower4.FailsToParse("abcDEF", "abcDEF", "4 Lowercase expected");


        var upper2 = Repeat(IsUpper, 2, "2 Uppercase");
        var upper3 = Repeat(IsUpper, 3, "3 Uppercase");
        var upper4 = Repeat(IsUpper, 4, "4 Uppercase");

        upper2.FailsToParse("abcDEF", "abcDEF", "2 Uppercase expected");
        upper3.FailsToParse("abcDEF", "abcDEF", "3 Uppercase expected");
        upper4.FailsToParse("abcDEF", "abcDEF", "4 Uppercase expected");

        upper2.PartiallyParses("DEF", "F").ShouldBe("DE");
        upper3.Parses("DEF").ShouldBe("DEF");
        upper4.FailsToParse("DEF", "DEF", "4 Uppercase expected");


        var caseInsensitive2 = Repeat(IsLetter, 2, "2 Case Insensitive");
        var caseInsensitive3 = Repeat(IsLetter, 3, "3 Case Insensitive");
        var caseInsensitive4 = Repeat(IsLetter, 4, "4 Case Insensitive");

        caseInsensitive2.FailsToParse("!abcDEF", "!abcDEF", "2 Case Insensitive expected");
        caseInsensitive3.FailsToParse("!abcDEF", "!abcDEF", "3 Case Insensitive expected");
        caseInsensitive4.FailsToParse("!abcDEF", "!abcDEF", "4 Case Insensitive expected");

        caseInsensitive2.PartiallyParses("abcDEF", "cDEF").ShouldBe("ab");
        caseInsensitive3.PartiallyParses("abcDEF", "DEF").ShouldBe("abc");
        caseInsensitive4.PartiallyParses("abcDEF", "EF").ShouldBe("abcD");


        var even = Repeat<int>(x => x % 2 == 0, 2, "2 even numbers");
        var empty = Array.Empty<int>();
        even.FailsToParse(empty, empty, "2 even numbers expected");
        even.FailsToParse(new[] { 1, 2, 4, 6 }, new[] { 1, 2, 4, 6 }, "2 even numbers expected");
        even.PartiallyParses(new[] { 2, 4, 6, 1, 3, 5 }, new[] { 6, 1, 3, 5 }).ShouldBe(new[] { 2, 4 });
        even.PartiallyParses(new[] { 4, 6, 1, 3, 5 }, new[] { 1, 3, 5 }).ShouldBe(new[] { 4, 6 });
        even.FailsToParse(new[] { 6, 1, 3, 5 }, new[] { 6, 1, 3, 5 }, "2 even numbers expected");
        even.PartiallyParses(new[] { 2, 4, 6 }, new[] { 6 }).ShouldBe(new[] { 2, 4 });
        even.Parses(new[] { 2, 4 }).ShouldBe(new[] { 2, 4 });


        var attemptRepeat0char = () => Repeat(IsLower, 0, "Lowercase");
        attemptRepeat0char
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Repeat requires the given count to be > 1. (Parameter 'count')");

        var attemptRepeat1char = () => Repeat(IsLower, 1, "Lowercase");
        attemptRepeat1char
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Repeat requires the given count to be > 1. (Parameter 'count')");


        var attemptRepeat0int = () => Repeat<int>(x => x % 2 == 0, 0, "even number");
        attemptRepeat0int
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Repeat requires the given count to be > 1. (Parameter 'count')");

        var attemptRepeat1int = () => Repeat<int>(x => x % 2 == 0, 1, "even number");
        attemptRepeat1int
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Repeat requires the given count to be > 1. (Parameter 'count')");
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

    public void ProvidesTheCurrentIndexPosition()
    {
        var index = Index<char>();

        var queryWithIndices =
            from _0 in index
            from ab in AB
            from _2 in index
            from a in A
            from b in B
            from _4 in index
            select new[] { _0, _2, _4 };

        queryWithIndices.Parses("ABAB").ShouldBe(new[] { 0, 2, 4 });
        queryWithIndices.PartiallyParses("ABABA", "A").ShouldBe(new[] { 0, 2, 4 });
    }

    public void ProvidesArbitraryInspectionAtTheCurrentIndexPosition()
    {
        var buildPositionTrackingParser = () =>
        {
            var metadata = TextPositionTracker();

            return
                from metadataAtStart in metadata
                from a in A
                from metadataBeforeWhitespace in metadata
                from whitespace in ZeroOrMore(IsWhiteSpace)
                from metadataAfterWhitespace in metadata
                from b in B
                from metadataAtEnd in metadata
                select new[]
                {
                    metadataAtStart,
                    metadataBeforeWhitespace,
                    metadataAfterWhitespace,
                    metadataAtEnd
                };
        };

        buildPositionTrackingParser().Parses("AB").ShouldBe(new[]
        {
            (0, 1, 1), //start
            (1, 1, 2), //before zero width whitespace
            (1, 1, 2), //after zero width whitespace
            (2, 1, 3)  //end
        });

        buildPositionTrackingParser().Parses("A \n \n   B").ShouldBe(new[]
        {
            (0, 1, 1), //start
            (1, 1, 2), //before whitespace
            (8, 3, 4), //after whitespace
            (9, 3, 5)  //end
        });
    }

    static Parser<char, (int index, int position, int endlines)> TextPositionTracker()
    {
        // This sample textual line/column tracker is naive in that it assumes
        // there will be no backtracking and therefore assumes that the single
        // lastMetadata value is sufficient to calculate a delta. In a parser
        // that includes backtracking, the lambda expression here should detect
        // whether or not lastMetadata is trustworthy and at least fall back
        // to counting lines from the origin.

        (int index, int line, int column) origin = (0, 1, 1);

        var lastMetadata = origin;

        return Inspect<char, (int index, int position, int endlines)>(
            (span, index) =>
            {
                var traversed = span.Slice(lastMetadata.index, index - lastMetadata.index);

                int lineDelta = 0;
                int columnDelta = 0;

                foreach (var ch in traversed)
                {
                    if (ch == '\n')
                    {
                        lineDelta++;
                        columnDelta = -lastMetadata.column;
                    }

                    columnDelta++;
                }

                var newMetadata = (index, lastMetadata.line + lineDelta, lastMetadata.column + columnDelta);

                lastMetadata = newMetadata;

                return newMetadata;
            });
    }
}

public class AlternationTests
{
    readonly Parser<char, string> A, B, C;

    public AlternationTests()
    {
        A = from c in Single('A') select c.ToString();
        B = from c in Single('B') select c.ToString();
        C = from c in Single('C') select c.ToString();
    }

    public void ChoosingRequiresAtLeastTwoParsersToChooseBetween()
    {
        var attemptChoiceBetweenZeroAlternatives = () => Choice<char, string>();
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

        Parser<char, string> succeedWithoutConsuming = (ReadOnlySpan<char> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            expectation = null;
            succeeded = true;
            return "atypical value";
        };

        Choice(A, succeedWithoutConsuming).Parses("").ShouldBe("atypical value");
        Choice(A, B, succeedWithoutConsuming).Parses("").ShouldBe("atypical value");
        Choice(A, succeedWithoutConsuming, B).Parses("").ShouldBe("atypical value");
    }

    static readonly Parser<char, string> NeverExecuted =
        (ReadOnlySpan<char> input, ref int index, out bool succeeded, out string? expectation)
            => throw new Exception("Parser 'NeverExecuted' should not have been executed.");
}
