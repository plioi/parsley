using System.Globalization;
using System.Text.RegularExpressions;
using static Parsley.Grammar;

namespace Parsley.Tests.IntegrationTests.Json;

public class JsonGrammar
{
    static readonly IParser<string> StringLiteral = Pattern("string", @"
            # Open quote:
            ""

            # Zero or more content characters:
            (
                      [^""\\]*             # Zero or more non-quote, non-slash characters.
                |     \\ [""\\bfnrt\/]     # One of: slash-quote   \\   \b   \f   \n   \r   \t   \/
                |     \\ u [0-9a-fA-F]{4}  # \u folowed by four hex digits
            )*

            # Close quote:
            ""
        ");

    static readonly IParser<string> NumberLiteral = Pattern("number", @"
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

    public static readonly GrammarRule<object> Json = new(nameof(Json));
    static readonly GrammarRule<object> JsonValue = new(nameof(JsonValue));

    static JsonGrammar()
    {
        var Whitespace = Optional(Pattern("whitespace", @"\s+"));

        var Comma = Symbol(",");
        var OpenArray = Symbol("[");
        var CloseArray = Symbol("]");
        var OpenDictionary = Symbol("{");
        var CloseDictionary = Symbol("}");
        var Colon = Symbol(":");

        var True =
            from _ in Keyword("true")
            from trailing in Whitespace
            select (object)true;

        var False =
            from _ in Keyword("false")
            from trailing in Whitespace
            select (object)false;

        var Null =
            from _ in Keyword("null")
            from trailing in Whitespace
            select (object)null;

        var Number =
            from number in NumberLiteral
            from trailing in Whitespace
            select (object) decimal.Parse(number, NumberStyles.Any, CultureInfo.InvariantCulture);

        var String =
            from quotation in StringLiteral
            from trailing in Whitespace
            select Unescape(quotation);

        var Array =
            from open in OpenArray
            from items in ZeroOrMore(JsonValue, Comma)
            from close in CloseArray
            select items.ToArray();

        var Pair =
            from key in String
            from colon in Colon
            from value in JsonValue
            select new KeyValuePair<string, object>(key, value);

        var Dictionary =
            from open in OpenDictionary
            from pairs in ZeroOrMore(Pair, Comma)
            from close in CloseDictionary
            select ToDictionary(pairs);

        JsonValue.Rule =
            Choice(True, False, Null, Number, String, Dictionary, Array);

        Json.Rule =
            from leading in Whitespace
            from jsonValue in JsonValue
            from end in EndOfInput
            from trailing in Whitespace
            select jsonValue;

        IParser<string> Symbol(string symbol) =>
            from s in Operator(symbol)
            from trailing in Whitespace
            select s;
    }

    static string Unescape(string quotation)
    {
        string result = quotation.Substring(1, quotation.Length - 2); //Remove leading and trailing quotation marks

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

    static Dictionary<string, object> ToDictionary(IEnumerable<KeyValuePair<string, object>> pairs)
    {
        var result = new Dictionary<string, object>();

        foreach (var pair in pairs)
            result[pair.Key] = pair.Value;

        return result;
    }
}
