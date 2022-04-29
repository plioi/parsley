using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Parsley;

public static class Assertions
{
    public static void FailsToParse<TValue>(this Parser<TValue> parse, string input, string expectedUnparsedInput, string expectedMessage)
    {
        ReadOnlySpan<char> inputSpan = input;
        Index index = new(0);

        if (parse(ref inputSpan, ref index, out var value, out var expectation))
            throw new AssertionException("parser failure", "parser completed successfully");

        var actual = expectation + " expected";
            
        if (actual != expectedMessage)
            throw new MessageAssertionException(expectedMessage, actual);
        
        if (expectedUnparsedInput == "")
            inputSpan.AtEndOfInput();
        else
            inputSpan.LeavingUnparsedInput(expectedUnparsedInput);
    }

    public static TValue PartiallyParses<TValue>(this Parser<TValue> parse, string input, string expectedUnparsedInput)
    {
        ReadOnlySpan<char> inputSpan = input;
        Index index = new(0);

        if (!parse(ref inputSpan, ref index, out var value, out var expectation))
            UnexpectedFailure(ref inputSpan, ref index, expectation);

        if (expectedUnparsedInput == "")
            throw new ArgumentException($"{nameof(expectedUnparsedInput)} must be nonempty when calling {nameof(PartiallyParses)}.");

        inputSpan.LeavingUnparsedInput(expectedUnparsedInput);

        return value;
    }

    public static TValue Parses<TValue>(this Parser<TValue> parse, string input)
    {
        ReadOnlySpan<char> inputSpan = input;
        Index index = new(0);

        if (!parse(ref inputSpan, ref index, out var value, out var expectation))
            UnexpectedFailure(ref inputSpan, ref index, expectation);

        inputSpan.AtEndOfInput();

        return value;
    }

    [DoesNotReturn]
    static void UnexpectedFailure(ref ReadOnlySpan<char> input, ref Index index, string expectation)
    {
        var peek = input.Peek(20).ToString();

        var offendingItem = peek[0];
        var displayFriendlyTrailingItems = new string(peek.Skip(1).TakeWhile(x => !char.IsControl(x)).ToArray());

        var message = new StringBuilder();
        message.AppendLine(index.ToString() + ": " + expectation + " expected");
        message.AppendLine();
        message.AppendLine($"\t{offendingItem}{displayFriendlyTrailingItems}");
        message.AppendLine("\t^");

        throw new AssertionException(message.ToString(), "parser success", "parser failure");
    }

    static void LeavingUnparsedInput(this ReadOnlySpan<char> input, string expectedUnparsedInput)
    {
        var actualUnparsedInput = input.ToString();

        if (actualUnparsedInput != expectedUnparsedInput)
            throw new AssertionException("Parse resulted in unexpected remaining unparsed input.",
                expectedUnparsedInput,
                actualUnparsedInput);
    }

    static void AtEndOfInput(this ReadOnlySpan<char> input)
    {
        if (!input.IsEmpty)
            throw new AssertionException("end of input", input.ToString());

        input.LeavingUnparsedInput("");
    }

    public static void ShouldBe(this Index actual, Index expected)
    {
        if (actual.Value != expected.Value)
            throw new AssertionException(expected.ToString(), actual.ToString());
    }
}
