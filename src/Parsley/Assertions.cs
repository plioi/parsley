using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Parsley;

public static class Assertions
{
    public static void FailsToParse<TValue>(this Parser<char, TValue> parse, string input, string expectedUnparsedInput, string expectedMessage)
    {
        if (parse.TryPartialParse(input, out int index, out var value, out var error))
            throw new AssertionException("parser failure", "parser completed successfully");

        var actual = error.Expectation + " expected";
            
        if (actual != expectedMessage)
            throw new MessageAssertionException(expectedMessage, actual);

        if (expectedUnparsedInput == "")
            input.AtEndOfInput(index);
        else
            input.LeavingUnparsedInput(index, expectedUnparsedInput);
    }

    public static TValue PartiallyParses<TValue>(this Parser<char, TValue> parse, string input, string expectedUnparsedInput)
    {
        if (expectedUnparsedInput == "")
            throw new ArgumentException($"{nameof(expectedUnparsedInput)} must be nonempty when calling {nameof(PartiallyParses)}.");

        if (!parse.TryPartialParse(input, out int index, out var value, out var error))
            UnexpectedFailure(input, error);

        input.LeavingUnparsedInput(index, expectedUnparsedInput);

        return value;
    }

    public static TValue Parses<TValue>(this Parser<char, TValue> parse, string input)
    {
        if (!parse.TryParse(input, out var value, out var error))
            UnexpectedFailure(input, error);

        return value;
    }

    [DoesNotReturn]
    static void UnexpectedFailure(ReadOnlySpan<char> input, ParseError error)
    {
        var message = new StringBuilder();
        var peek = input.Peek(error.Index, 20).ToString();

        if (peek.Length > 0)
        {
            var offendingItem = peek[0];
            var displayFriendlyTrailingItems = new string(peek.Skip(1).TakeWhile(x => !char.IsControl(x)).ToArray());

            message.AppendLine(error.Index + ": " + error.Expectation + " expected");
            message.AppendLine();
            message.AppendLine($"\t{offendingItem}{displayFriendlyTrailingItems}");
            message.AppendLine("\t^");
        }
        else
        {
            message.AppendLine(error.Index + ": " + error.Expectation + " expected");
        }

        throw new AssertionException(message.ToString(), "parser success", "parser failure");
    }

    static void LeavingUnparsedInput(this string input, int index, string expectedUnparsedInput)
    {
        var actualUnparsedInput = input.Substring(index);

        if (actualUnparsedInput != expectedUnparsedInput)
            throw new AssertionException("Parse resulted in unexpected remaining unparsed input.",
                expectedUnparsedInput,
                actualUnparsedInput);
    }

    static void AtEndOfInput(this string input, int index)
    {
        if (index != input.Length)
            throw new AssertionException("end of input", input.Substring(index));

        input.LeavingUnparsedInput(index, "");
    }
}
