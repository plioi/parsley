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

    public void ApplyingARuleBetweenTwoOtherRules()
    {
        var parser = Between(A, B, A);

        parser.FailsToParse("", "", "A expected");
        parser.FailsToParse("B", "B", "A expected");
        parser.FailsToParse("A", "", "B expected");
        parser.FailsToParse("AA", "A", "B expected");
        parser.FailsToParse("AB", "", "A expected");
        parser.FailsToParse("ABB", "B", "A expected");
        parser.Parses("ABA").ShouldBe('B');
        parser.PartiallyParses("ABA!", "!").ShouldBe('B');
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
        var lower = ZeroOrMore(IsLower, span => span.ToString());
        var upper = ZeroOrMore(IsUpper, span => span.ToString());
        var caseInsensitive = ZeroOrMore(IsLetter, span => span.ToString());

        lower.Parses("").ShouldBe("");

        lower.PartiallyParses("ABCdef", "ABCdef").ShouldBe("");

        upper.PartiallyParses("abcDEF", "abcDEF").ShouldBe("");

        caseInsensitive.PartiallyParses("!abcDEF", "!abcDEF").ShouldBe("");

        lower.PartiallyParses("abcDEF", "DEF").ShouldBe("abc");

        upper.Parses("DEF").ShouldBe("DEF");

        caseInsensitive.Parses("abcDEF").ShouldBe("abcDEF");

        var isEven = (int x) => x % 2 == 0;
        var even = ZeroOrMore(isEven, span => span.ToArray());
        var empty = Array.Empty<int>();
        even.Parses(empty).ShouldBe(empty);
        even.PartiallyParses(new[] { 1, 2, 4, 6 }, new[] { 1, 2, 4, 6 }).ShouldBe(empty);
        even.PartiallyParses(new[] { 2, 4, 6, 1, 3, 5 }, new[] { 1, 3, 5 }).ShouldBe(new[] { 2, 4, 6 });
        even.Parses(new[] { 2, 4, 6 }).ShouldBe(new[] { 2, 4, 6 });
    }

    public void ProvidesConveniencePrimitiveRecognizingNonemptySequencesOfItemsSatisfyingSomePredicate()
    {
        var lower = OneOrMore(IsLower, "Lowercase", span => span.ToString());
        var upper = OneOrMore(IsUpper, "Uppercase", span => span.ToString());
        var caseInsensitive = OneOrMore(IsLetter, "Case Insensitive", span => span.ToString());

        lower.FailsToParse("", "", "Lowercase expected");
        
        lower.FailsToParse("ABCdef", "ABCdef", "Lowercase expected");

        upper.FailsToParse("abcDEF", "abcDEF", "Uppercase expected");

        caseInsensitive.FailsToParse("!abcDEF", "!abcDEF", "Case Insensitive expected");

        lower.PartiallyParses("abcDEF", "DEF").ShouldBe("abc");

        upper.Parses("DEF").ShouldBe("DEF");

        caseInsensitive.Parses("abcDEF").ShouldBe("abcDEF");

        var isEven = (int x) => x % 2 == 0;
        var even = OneOrMore(isEven, "even number", span => span.ToArray());
        var empty = Array.Empty<int>();
        even.FailsToParse(empty, empty, "even number expected");
        even.FailsToParse(new[] { 1, 2, 4, 6 }, new[] { 1, 2, 4, 6 }, "even number expected");
        even.PartiallyParses(new[] { 2, 4, 6, 1, 3, 5 }, new[] { 1, 3, 5 }).ShouldBe(new[] { 2, 4, 6 });
        even.Parses(new[] { 2, 4, 6 }).ShouldBe(new[] { 2, 4, 6 });
    }

    public void ProvidesConveniencePrimitiveRecognizingSequencesOfItemsSatisfyingSomePredicateAFixedNumberOfTimes()
    {
        var lower2 = Repeat(IsLower, 2, "2 Lowercase", span => span.ToString());
        var lower3 = Repeat(IsLower, 3, "3 Lowercase", span => span.ToString());
        var lower4 = Repeat(IsLower, 4, "4 Lowercase", span => span.ToString());

        lower2.FailsToParse("", "", "2 Lowercase expected");
        lower3.FailsToParse("", "", "3 Lowercase expected");
        lower4.FailsToParse("", "", "4 Lowercase expected");

        lower2.FailsToParse("ABCdef", "ABCdef", "2 Lowercase expected");
        lower3.FailsToParse("ABCdef", "ABCdef", "3 Lowercase expected");
        lower4.FailsToParse("ABCdef", "ABCdef", "4 Lowercase expected");

        lower2.PartiallyParses("abcDEF", "cDEF").ShouldBe("ab");
        lower3.PartiallyParses("abcDEF", "DEF").ShouldBe("abc");
        lower4.FailsToParse("abcDEF", "abcDEF", "4 Lowercase expected");


        var upper2 = Repeat(IsUpper, 2, "2 Uppercase", span => span.ToString());
        var upper3 = Repeat(IsUpper, 3, "3 Uppercase", span => span.ToString());
        var upper4 = Repeat(IsUpper, 4, "4 Uppercase", span => span.ToString());

        upper2.FailsToParse("abcDEF", "abcDEF", "2 Uppercase expected");
        upper3.FailsToParse("abcDEF", "abcDEF", "3 Uppercase expected");
        upper4.FailsToParse("abcDEF", "abcDEF", "4 Uppercase expected");

        upper2.PartiallyParses("DEF", "F").ShouldBe("DE");
        upper3.Parses("DEF").ShouldBe("DEF");
        upper4.FailsToParse("DEF", "DEF", "4 Uppercase expected");


        var caseInsensitive2 = Repeat(IsLetter, 2, "2 Case Insensitive", span => span.ToString());
        var caseInsensitive3 = Repeat(IsLetter, 3, "3 Case Insensitive", span => span.ToString());
        var caseInsensitive4 = Repeat(IsLetter, 4, "4 Case Insensitive", span => span.ToString());

        caseInsensitive2.FailsToParse("!abcDEF", "!abcDEF", "2 Case Insensitive expected");
        caseInsensitive3.FailsToParse("!abcDEF", "!abcDEF", "3 Case Insensitive expected");
        caseInsensitive4.FailsToParse("!abcDEF", "!abcDEF", "4 Case Insensitive expected");

        caseInsensitive2.PartiallyParses("abcDEF", "cDEF").ShouldBe("ab");
        caseInsensitive3.PartiallyParses("abcDEF", "DEF").ShouldBe("abc");
        caseInsensitive4.PartiallyParses("abcDEF", "EF").ShouldBe("abcD");

        var isEven = (int x) => x % 2 == 0;
        var even = Repeat(isEven, 2, "2 even numbers", span => span.ToArray());
        var empty = Array.Empty<int>();
        even.FailsToParse(empty, empty, "2 even numbers expected");
        even.FailsToParse(new[] { 1, 2, 4, 6 }, new[] { 1, 2, 4, 6 }, "2 even numbers expected");
        even.PartiallyParses(new[] { 2, 4, 6, 1, 3, 5 }, new[] { 6, 1, 3, 5 }).ShouldBe(new[] { 2, 4 });
        even.PartiallyParses(new[] { 4, 6, 1, 3, 5 }, new[] { 1, 3, 5 }).ShouldBe(new[] { 4, 6 });
        even.FailsToParse(new[] { 6, 1, 3, 5 }, new[] { 6, 1, 3, 5 }, "2 even numbers expected");
        even.PartiallyParses(new[] { 2, 4, 6 }, new[] { 6 }).ShouldBe(new[] { 2, 4 });
        even.Parses(new[] { 2, 4 }).ShouldBe(new[] { 2, 4 });


        var attemptRepeat0char = () => Repeat(IsLower, 0, "Lowercase", span => span.ToString());
        attemptRepeat0char
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Repeat requires the given count to be > 1. (Parameter 'count')");

        var attemptRepeat1char = () => Repeat(IsLower, 1, "Lowercase", span => span.ToString());
        attemptRepeat1char
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Repeat requires the given count to be > 1. (Parameter 'count')");

        var attemptRepeat0int = () => Repeat(isEven, 0, "even number", span => span.ToArray());
        attemptRepeat0int
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Repeat requires the given count to be > 1. (Parameter 'count')");

        var attemptRepeat1int = () => Repeat(isEven, 1, "even number", span => span.ToArray());
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
                from whitespace in ZeroOrMore(IsWhiteSpace, span => span.ToString())
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

    public void ProvidesLastMissedExpectationWhenParsersFailWithoutConsumingInput()
    {
        Choice(A, B).FailsToParse("", "", "B expected");
        Choice(A, B, C).FailsToParse("", "", "C expected");
    }

    public void SupportsOptionalComprehensiveExpectationsWhenAllParsersFailWithoutConsumingInput()
    {
        Choice("A or B", A, B).FailsToParse("", "", "A or B expected");
        Choice("A, B, or C", A, B, C).FailsToParse("", "", "A, B, or C expected");
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

    public void SpecificExpectationSupercedesComprehensiveExpectationWhenAnyParserFailsAfterConsumingInput()
    {
        //As soon as something consumes input, it's failure and message win.

        var AB = from a in A
            from b in B
            select $"{a}{b}";

        Choice("This expectation is discarded.", AB, NeverExecuted).FailsToParse("A", "", "B expected");
        Choice("This expectation is discarded.", C, AB, NeverExecuted).FailsToParse("A", "", "B expected");
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
