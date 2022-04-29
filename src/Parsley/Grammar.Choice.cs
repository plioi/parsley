using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// Choice() with zero parsers is an invalid request, and will
    /// throw an exception.
    /// 
    /// Choice(p) with one parser would be wastefully equivalent to p,
    /// and will throw an exception.
    /// 
    /// For 2 or more inputs, parsers are applied from left
    /// to right.  If a parser succeeds, that result wins.
    /// If a parser fails without consuming input, the next parser
    /// is attempted.  If a parser fails after consuming input,
    /// subsequent parsers will not be attempted. As long as
    /// parsers consume no input, their error messages are merged.
    ///
    /// Choice is 'predictive' since p[n+1] is only tried when
    /// p[n] didn't consume any input (i.e. the look-ahead is 1).
    /// This non-backtracking behaviour allows for both an efficient
    /// implementation of the parser combinators and the generation
    /// of good error messages.
    /// </summary>
    public static Parser<TValue> Choice<TValue>(params Parser<TValue>[] parsers)
    {
        if (parsers.Length <= 1)
            throw new ArgumentException(
                $"{nameof(Choice)} requires at least two parsers to choose between.", nameof(parsers));

        return (ref ReadOnlySpan<char> input, ref int position, [NotNullWhen(true)] out TValue? value, [NotNullWhen(false)] out string? expectation) =>
        {
            var originalInput = input;

            var expectations = new List<string>();

            foreach (var parser in parsers)
            {
                if (parser(ref input, ref position, out value, out expectation))
                    return true;

                if (originalInput != input)
                    return false;

                expectations.Add(expectation);
            }

            expectation = CompoundExpectation(expectations);
            value = default;
            return false;
        };
    }

    static string CompoundExpectation(IReadOnlyList<string> expectations)
    {
        if (expectations.Count == 1)
            return expectations[0];

        if (expectations.Count == 2)
            return $"({expectations[0]} or {expectations[1]})";

        var combined = new StringBuilder();

        combined.Append('(');

        for (var i = 0; i < expectations.Count; i++)
        {
            if (i == 0)
            {
                combined.Append(expectations[i]);
            }
            else
            {
                var separator = i == expectations.Count - 1
                    ? ", or "
                    : ", ";

                combined.Append(separator + expectations[i]);
            }
        }

        combined.Append(')');

        return combined.ToString();
    }
}
