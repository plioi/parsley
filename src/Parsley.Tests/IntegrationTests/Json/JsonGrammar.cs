using System.Globalization;
using System.Text.RegularExpressions;
using static Parsley.Grammar;

namespace Parsley.Tests.IntegrationTests.Json;

public class JsonGrammar
{
    public static readonly Parser<object> JsonDocument;

    static readonly Parser<object> Value;

    static JsonGrammar()
    {
        var Whitespace = Optional(OneOrMore(char.IsWhiteSpace, "whitespace"));

        var Key =
            from leading in Whitespace
            from quote in Quote
            from trailing in Whitespace
            select Unquote(quote);

        var Pair =
            from key in Key
            from colon in Operator(":")
            from value in Value
            select new KeyValuePair<string, object>(key, value);

        Value =
            from leading in Whitespace
            from value in Choice(
                from @true in Keyword("true")
                select (object) true,

                from @false in Keyword("false")
                select (object) false,

                from @null in Keyword("null")
                select (object) null,

                from number in Number
                select (object) decimal.Parse(number, NumberStyles.Any, CultureInfo.InvariantCulture),

                from quotation in Quote
                select Unquote(quotation),

                from open in Operator("{")
                from pairs in ZeroOrMore(Pair, Operator(","))
                from close in Operator("}")
                select pairs.ToDictionary(x => x.Key, x => x.Value),

                from open in Operator("[")
                from items in ZeroOrMore(Value, Operator(","))
                from close in Operator("]")
                select items.ToArray()
            )
            from trailing in Whitespace
            select value;

        JsonDocument =
            from value in Value
            from end in EndOfInput
            select value;
    }

    static readonly Parser<string> Number = Pattern("number", @"
            # Look-ahead to confirm the whole-number part is either 0 or starts with 1-9:
            (?=
                0(?!\d)  |  [1-9]
            )

            # Whole number part:
            \d+

            # Optional fractional part:
            (\.\d+)?

            # Optional exponent
            (
                [eE]
                [+-]?
                \d+
            )?
        ");

    static readonly Parser<string> Quote = Pattern("string", @"
            # Open quote:
            ""

            # Zero or more content characters:
            (
                      [^""\\]*             # Zero or more non-quote, non-slash characters.
                |     \\ [""\\bfnrt\/]     # One of: slash-quote   \\   \b   \f   \n   \r   \t   \/
                |     \\ u [0-9a-fA-F]{4}  # \u followed by four hex digits
            )*

            # Close quote:
            ""
        ");

    static string Unquote(string quote)
    {
        string result = quote.Substring(1, quote.Length - 2); //Remove leading and trailing quotation marks

        result = Regex.Replace(result, @"\\u[0-9a-fA-F]{4}",
            match => char.ConvertFromUtf32(int.Parse(match.Value.Replace("\\u", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture)));

        result = result
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\")
            .Replace("\\b", "\b")
            .Replace("\\f", "\f")
            .Replace("\\n", "\n")
            .Replace("\\r", "\r")
            .Replace("\\t", "\t")
            .Replace("\\/", "/");

        return result;
    }
}
