using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Parsley;

public static class Assertions
{
    public static void FailsToParse<T>(this Parser<T> parse, string input, string expectedUnparsedInput, string expectedMessage)
    {
        var text = new Text(input);

        if (parse(ref text, out var value, out var expectation))
            throw new AssertionException("parser failure", "parser completed successfully");

        var actual = expectation + " expected";
            
        if (actual != expectedMessage)
            throw new MessageAssertionException(expectedMessage, actual);
        
        if (expectedUnparsedInput == "")
            text.AtEndOfInput();
        else
            text.LeavingUnparsedInput(expectedUnparsedInput);
    }

    public static T PartiallyParses<T>(this Parser<T> parse, string input, string expectedUnparsedInput)
    {
        var text = new Text(input);

        if (!parse(ref text, out var value, out var expectation))
            UnexpectedFailure(ref text, expectation);

        if (expectedUnparsedInput == "")
            throw new ArgumentException($"{nameof(expectedUnparsedInput)} must be nonempty when calling {nameof(PartiallyParses)}.");

        text.LeavingUnparsedInput(expectedUnparsedInput);

        return value;
    }

    public static T Parses<T>(this Parser<T> parse, string input)
    {
        var text = new Text(input);

        if (!parse(ref text, out var value, out var expectation))
            UnexpectedFailure(ref text, expectation);

        text.AtEndOfInput();

        return value;
    }

    [DoesNotReturn]
    static void UnexpectedFailure(ref Text text, string expectation)
    {
        var peek = text.Peek(20).ToString();

        var offendingCharacter = peek[0];
        var displayFriendlyTrailingCharacters = new string(peek.Skip(1).TakeWhile(x => !char.IsControl(x)).ToArray());

        var message = new StringBuilder();
        message.AppendLine(text.Position + ": " + expectation + " expected");
        message.AppendLine();
        message.AppendLine($"\t{offendingCharacter}{displayFriendlyTrailingCharacters}");
        message.AppendLine("\t^");

        throw new AssertionException(message.ToString(), "parser success", "parser failure");
    }

    static void LeavingUnparsedInput(this Text text, string expectedUnparsedInput)
    {
        var actualUnparsedInput = text.ToString();

        if (actualUnparsedInput != expectedUnparsedInput)
            throw new AssertionException("Parse resulted in unexpected remaining unparsed input.",
                expectedUnparsedInput,
                actualUnparsedInput);
    }

    static void AtEndOfInput(this Text text)
    {
        if (!text.EndOfInput)
            throw new AssertionException("end of input", text.ToString());

        text.LeavingUnparsedInput("");
    }
}
