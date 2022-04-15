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
        A = Character('A');
        B = Character('B');

        AB = from a in A
            from b in B
            select $"{a}{b}";

        COMMA = Character(',');
    }

    public void CanFailWithoutConsumingInput()
    {
        Grammar<string>.Fail.FailsToParse("ABC", "ABC", "(1, 1): Parse error.");
    }

    public void CanDetectTheEndOfInputWithoutAdvancing()
    {
        EndOfInput.Parses("").Value.ShouldBe("");
        EndOfInput.FailsToParse("!", "!", "(1, 1): end of input expected");
    }

    public void CanDemandThatAGivenParserRecognizesTheNextConsumableInput()
    {
        Letter.Parses("A").Value.ShouldBe('A');
        Letter.FailsToParse("0", "0", "(1, 1): Letter expected");

        Digit.FailsToParse("A", "A", "(1, 1): Digit expected");
        Digit.Parses("0").Value.ShouldBe('0');
    }

    public void CanDemandThatAGivenTokenLiteralAppearsNext()
    {
        A.Parses("A").Value.ShouldBe('A');
        A.PartiallyParses("A!", "!").Value.ShouldBe('A');
        A.FailsToParse("B", "B", "(1, 1): A expected");
    }

    public void ApplyingARuleZeroOrMoreTimes()
    {
        var parser = ZeroOrMore(AB);

        parser.Parses("", "(1, 1): A expected").Value.ShouldBeEmpty();

        parser.PartiallyParses("AB!", "!", "(1, 3): A expected")
            .Value.Single().ShouldBe("AB");

        parser.PartiallyParses("ABAB!", "!", "(1, 5): A expected")
            .Value.ShouldBe(new[] { "AB", "AB" });

        parser.FailsToParse("ABABA!", "!", "(1, 6): B expected");

        Parser<string> succeedWithoutConsuming = (ref Text input) => new Parsed<string>("ignored value", input.Position);
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

        parser.FailsToParse("", "", "(1, 1): A expected");

        parser.PartiallyParses("AB!", "!")
            .Value.Single().ShouldBe("AB");

        parser.PartiallyParses("ABAB!", "!")
            .Value.ShouldBe(new[] { "AB", "AB" });

        parser.FailsToParse("ABABA!", "!", "(1, 6): B expected");

        Parser<string> succeedWithoutConsuming = (ref Text input) => new Parsed<string>("ignored value", input.Position);
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

        parser.Parses("", "(1, 1): A expected").Value.ShouldBeEmpty();
        parser.Parses("AB").Value.Single().ShouldBe("AB");
        parser.Parses("AB,AB").Value.ShouldBe(new[] { "AB", "AB" });
        parser.Parses("AB,AB,AB").Value.ShouldBe(new[] { "AB", "AB", "AB" });
        parser.FailsToParse("AB,", "", "(1, 4): A expected");
        parser.FailsToParse("AB,A", "", "(1, 5): B expected");
    }

    public void ApplyingARuleOneOrMoreTimesInterspersedByASeparatorRule()
    {
        var parser = OneOrMore(AB, COMMA);

        parser.FailsToParse("", "", "(1, 1): A expected");
        parser.Parses("AB").Value.Single().ShouldBe("AB");
        parser.Parses("AB,AB").Value.ShouldBe(new[] { "AB", "AB" });
        parser.Parses("AB,AB,AB").Value.ShouldBe(new[] { "AB", "AB", "AB" });
        parser.FailsToParse("AB,", "", "(1, 4): A expected");
        parser.FailsToParse("AB,A", "", "(1, 5): B expected");
    }

    public void ParsingAnOptionalRuleZeroOrOneTimes()
    {
        //Reference Type to Nullable Reference Type
        Optional(AB).PartiallyParses("AB.", ".").Value.ShouldBe("AB");
        Optional(AB).PartiallyParses(".", ".", "(1, 1): A expected").Value.ShouldBe(null);
        Optional(AB).FailsToParse("AC.", "C.", "(1, 2): B expected");

        //Value Type to Nullable Value Type
        Optional(A).PartiallyParses("AB.", "B.").Value.ShouldBe('A');
        Optional(A).PartiallyParses(".", ".", "(1, 1): A expected").Value.ShouldBe(null);
        Optional(B).PartiallyParses("A", "A", "(1, 1): B expected").Value.ShouldBe(null);
        Optional(B).Parses("", "(1, 1): B expected").Value.ShouldBe(null);

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
        Attempt(AB).Parses("AB").Value.ShouldBe("AB");

        //When p fails without consuming input, Attempt(p) is the same as p.
        Attempt(AB).FailsToParse("!", "!", "(1, 1): A expected");

        //When p fails after consuming input, Attempt(p) backtracks before reporting failure.
        Attempt(AB).FailsToParse("A!", "A!", "(1, 1): [(1, 2): B expected]");
    }

    public void ImprovingDefaultMessagesWithAKnownExpectation()
    {
        var labeled = Label(AB, "'A' followed by 'B'");

        //When p succeeds after consuming input, Label(p) is the same as p.
        AB.Parses("AB").Value.ShouldBe("AB");
        labeled.Parses("AB").Value.ShouldBe("AB");

        //When p fails after consuming input, Label(p) is the same as p.
        AB.FailsToParse("A!", "!", "(1, 2): B expected");
        labeled.FailsToParse("A!", "!", "(1, 2): B expected");

        //When p succeeds but does not consume input, Label(p) still succeeds but the potential error is included.
        var succeedWithoutConsuming = "$".SucceedWithThisValue();
        succeedWithoutConsuming
            .PartiallyParses("!", "!")
            .Value.ShouldBe("$");
        Label(succeedWithoutConsuming, "nothing")
            .PartiallyParses("!", "!", "(1, 1): nothing expected")
            .Value.ShouldBe("$");

        //When p fails but does not consume input, Label(p) fails with the given expectation.
        AB.FailsToParse("!", "!", "(1, 1): A expected");
        labeled.FailsToParse("!", "!", "(1, 1): 'A' followed by 'B' expected");
    }

    public void ProvidesConveniencePrimitiveRecognizingOneExpectedCharacter()
    {
        var x = Character('x');

        x.FailsToParse("", "", "(1, 1): x expected");
        x.FailsToParse("yz", "yz", "(1, 1): x expected");
        x.PartiallyParses("xyz", "yz").Value.ShouldBe('x');
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

        lower.FailsToParse("", "", "(1, 1): Lowercase expected");
        
        lower.FailsToParse("ABCdef", "ABCdef", "(1, 1): Lowercase expected");

        upper.FailsToParse("abcDEF", "abcDEF", "(1, 1): Uppercase expected");

        caseInsensitive.FailsToParse("!abcDEF", "!abcDEF", "(1, 1): Case Insensitive expected");

        lower.PartiallyParses("abcDEF", "DEF").Value.ShouldBe("abc");

        upper.Parses("DEF").Value.ShouldBe("DEF");

        caseInsensitive.Parses("abcDEF").Value.ShouldBe("abcDEF");
    }

    public void ProvidesConveniencePrimitiveForDefiningKeywords()
    {
        var foo = Keyword("foo");

        foo.FailsToParse("", "", "(1, 1): foo expected");
        
        foo.FailsToParse("bar", "bar", "(1, 1): foo expected");
        foo.FailsToParse("fo", "fo", "(1, 1): foo expected");

        foo.PartiallyParses("foo ", " ").Value.ShouldBe("foo");
        foo.Parses("foo").Value.ShouldBe("foo");

        foo.PartiallyParses("foo bar", " bar").Value.ShouldBe("foo");

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
            .Value.ShouldBe("*");

        star.PartiallyParses("* *", " *")
            .Value.ShouldBe("*");

        star.PartiallyParses("**", "*")
            .Value.ShouldBe("*");

        doubleStar.FailsToParse("a", "a", "(1, 1): ** expected");

        doubleStar.FailsToParse("*", "*", "(1, 1): ** expected");

        doubleStar.FailsToParse("* *", "* *", "(1, 1): ** expected");

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

    public void ChoosingBetweenZeroAlternativesAlwaysFails()
    {
        Choice<string>().FailsToParse("ABC", "ABC", "(1, 1): Parse error.");
    }

    public void ChoosingBetweenOneAlternativeParserIsEquivalentToThatParser()
    {
        Choice(A).Parses("A").Value.ShouldBe("A");
        Choice(A).PartiallyParses("AB", "B").Value.ShouldBe("A");
        Choice(A).FailsToParse("B", "B", "(1, 1): A expected");
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

        Choice(AB, NeverExecuted).FailsToParse("A", "", "(1, 2): B expected");
        Choice(C, AB, NeverExecuted).FailsToParse("A", "", "(1, 2): B expected");
    }

    public void MergesErrorMessagesWhenParsersFailWithoutConsumingInput()
    {
        Choice(A, B).FailsToParse("", "", "(1, 1): A or B expected");
        Choice(A, B, C).FailsToParse("", "", "(1, 1): A, B or C expected");
    }

    public void MergesPotentialErrorMessagesWhenParserSucceedsWithoutConsumingInput()
    {
        //Choice really shouldn't be used with parsers that can succeed without
        //consuming input. These tests simply describe the behavior under that
        //unusual situation.

        Parser<string> succeedWithoutConsuming = (ref Text input) => new Parsed<string>("atypical value", input.Position);

        Choice(A, succeedWithoutConsuming).Parses("", "(1, 1): A expected").Value.ShouldBe("atypical value");
        Choice(A, B, succeedWithoutConsuming).Parses("", "(1, 1): A or B expected").Value.ShouldBe("atypical value");
        Choice(A, succeedWithoutConsuming, B).Parses("", "(1, 1): A expected").Value.ShouldBe("atypical value");
    }

    static readonly Parser<string> NeverExecuted =
        (ref Text input) => throw new Exception("Parser 'NeverExecuted' should not have been executed.");
}
