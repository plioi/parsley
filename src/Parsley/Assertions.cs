using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Parsley;

public static class Assertions
{
    public static void FailsToParse<TItem, TValue>(this Parser<TItem, TValue> parse, ReadOnlySpan<TItem> input, ReadOnlySpan<TItem> expectedUnparsedInput, string expectedMessage)
    {
        if (parse.TryPartialParse(input, out int index, out var value, out var error))
            throw new AssertionException("parser failure", "parser completed successfully");

        var actual = error.Expectation + " expected";
            
        if (actual != expectedMessage)
            throw new MessageAssertionException(expectedMessage, actual);

        if (expectedUnparsedInput.IsEmpty)
            input.AtEndOfInput(index);
        else
            input.LeavingUnparsedInput(index, expectedUnparsedInput);
    }

    public static TValue PartiallyParses<TItem, TValue>(this Parser<TItem, TValue> parse, ReadOnlySpan<TItem> input, ReadOnlySpan<TItem> expectedUnparsedInput)
    {
        if (expectedUnparsedInput.IsEmpty)
            throw new ArgumentException($"{nameof(expectedUnparsedInput)} must be nonempty when calling {nameof(PartiallyParses)}.");

        if (!parse.TryPartialParse(input, out int index, out var value, out var error))
            UnexpectedFailure(input, error);

        input.LeavingUnparsedInput(index, expectedUnparsedInput);

        return value;
    }

    public static TValue Parses<TItem, TValue>(this Parser<TItem, TValue> parse, ReadOnlySpan<TItem> input)
    {
        if (!parse.TryParse(input, out var value, out var error))
            UnexpectedFailure(input, error);

        return value;
    }

    [DoesNotReturn]
    static void UnexpectedFailure<TItem>(ReadOnlySpan<TItem> input, ParseError error)
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

    static void LeavingUnparsedInput<TItem>(this ReadOnlySpan<TItem> input, int index, ReadOnlySpan<TItem> expectedUnparsedInput)
    {
        var actualUnparsedInput = input.Slice(index);

        if (!actualUnparsedInput.SequenceEqual(expectedUnparsedInput))
            throw new AssertionException("Parse resulted in unexpected remaining unparsed input.",
                Display(expectedUnparsedInput),
                Display(actualUnparsedInput));
    }

    static void AtEndOfInput<TItem>(this ReadOnlySpan<TItem> input, int index)
    {
        if (index != input.Length)
        {
            var unparsedInput = input.Slice(index);

            throw new AssertionException("end of input", Display(unparsedInput));
        }

        input.LeavingUnparsedInput(index, Array.Empty<TItem>());
    }

    static string Display<TItem>(ReadOnlySpan<TItem> unparsedInput)
        => typeof(TItem) == typeof(char)
            ? unparsedInput.ToString()
            : $"[{string.Join(", ", unparsedInput.ToArray().Select(x => x?.ToString()))}]";
}
