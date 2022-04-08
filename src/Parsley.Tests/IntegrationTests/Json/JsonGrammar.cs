using System.Globalization;
using System.Text.RegularExpressions;
using static Parsley.Grammar;

namespace Parsley.Tests.IntegrationTests.Json;

public class JsonGrammar
{
    static readonly Pattern WhitespaceLiteral = new("whitespace", @"\s+");
    static readonly Keyword @null = new("null");
    static readonly Keyword @true = new("true");
    static readonly Keyword @false = new("false");
    static readonly Operator Comma = new(",");
    static readonly Operator OpenArray = new("[");
    static readonly Operator CloseArray = new("]");
    static readonly Operator OpenDictionary = new("{");
    static readonly Operator CloseDictionary = new("}");
    static readonly Operator Colon = new(":");

    static readonly Pattern StringLiteral = new("string", @"
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

    static readonly Pattern NumberLiteral = new("number", @"
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
    static readonly GrammarRule<Token> Whitespace = new(nameof(Whitespace));
    static readonly GrammarRule<object> JsonValue = new(nameof(JsonValue));
    static readonly GrammarRule<object> True = new(nameof(True));
    static readonly GrammarRule<object> False = new(nameof(False));
    static readonly GrammarRule<object> Null = new(nameof(Null));
    static readonly GrammarRule<object> Number = new(nameof(Number));
    static readonly GrammarRule<string> String = new(nameof(String));
    static readonly GrammarRule<object[]> Array = new(nameof(Array));
    static readonly GrammarRule<KeyValuePair<string, object>> Pair = new(nameof(Pair));
    static readonly GrammarRule<Dictionary<string, object>> Dictionary = new(nameof(Dictionary));

    static JsonGrammar()
    {
        Whitespace.Rule =
            Optional(Token(WhitespaceLiteral));

        True.Rule =
            Keyword(@true, true);

        False.Rule =
            Keyword(@false, false);

        Null.Rule =
            Keyword(@null, null);

        Number.Rule =
            from number in Token(NumberLiteral)
            from trailing in Whitespace
            select (object) decimal.Parse(number.Literal, NumberStyles.Any, CultureInfo.InvariantCulture);

        String.Rule =
            from quotation in Token(StringLiteral)
            from trailing in Whitespace
            select Unescape(quotation.Literal);

        Array.Rule =
            from items in Between(Operator(OpenArray), ZeroOrMore(JsonValue, Operator(Comma)), Operator(CloseArray))
            select items.ToArray();

        Pair.Rule =
            from key in String
            from colon in Operator(Colon)
            from value in JsonValue
            select new KeyValuePair<string, object>(key, value);

        Dictionary.Rule =
            from pairs in Between(Operator(OpenDictionary), ZeroOrMore(Pair, Operator(Comma)), Operator(CloseDictionary))
            select ToDictionary(pairs);

        JsonValue.Rule = Choice(True, False, Null, Number, String, Dictionary, Array);

        Json.Rule =
            from leading in Whitespace
            from jsonValue in JsonValue
            from end in EndOfInput
            from trailing in Whitespace
            select jsonValue;
    }

    static IParser<object> Keyword(Keyword keyword, object constant)
    {
        return from _ in Token(keyword)
            from trailing in Whitespace
            select constant;
    }

    static IParser<object> Operator(Operator @operator)
    {
        return from symbol in Token(@operator)
            from trailing in Whitespace
            select symbol;
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
