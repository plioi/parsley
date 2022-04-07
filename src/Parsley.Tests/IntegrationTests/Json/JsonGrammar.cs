using System.Globalization;
using System.Text.RegularExpressions;
using static Parsley.Grammar;

namespace Parsley.Tests.IntegrationTests.Json;

public class JsonGrammar
{
    public static readonly GrammarRule<object> Json = new(nameof(Json));
    static readonly GrammarRule<object> JsonValue = new(nameof(JsonValue));
    static readonly GrammarRule<object> True = new(nameof(True));
    static readonly GrammarRule<object> False = new(nameof(False));
    static readonly GrammarRule<object> Null = new(nameof(Null));
    static readonly GrammarRule<object> Number = new(nameof(Number));
    static readonly GrammarRule<string> Quotation = new(nameof(Quotation));
    static readonly GrammarRule<object[]> Array = new(nameof(Array));
    static readonly GrammarRule<KeyValuePair<string, object>> Pair = new(nameof(Pair));
    static readonly GrammarRule<Dictionary<string, object>> Dictionary = new(nameof(Dictionary));

    static JsonGrammar()
    {
        True.Rule =
            Constant(JsonLexer.@true, true);

        False.Rule =
            Constant(JsonLexer.@false, false);

        Null.Rule =
            Constant(JsonLexer.@null, null);

        Number.Rule =
            from number in Token(JsonLexer.Number)
            select (object) Decimal.Parse(number.Literal, NumberStyles.Any, CultureInfo.InvariantCulture);

        Quotation.Rule =
            from quotation in Token(JsonLexer.Quotation)
            select Unescape(quotation.Literal);

        Array.Rule =
            from items in Between(Token("["), ZeroOrMore(JsonValue, Token(",")), Token("]"))
            select items.ToArray();

        Pair.Rule =
            from key in Quotation
            from colon in Token(":")
            from value in JsonValue
            select new KeyValuePair<string, object>(key, value);

        Dictionary.Rule =
            from pairs in Between(Token("{"), ZeroOrMore(Pair, Token(",")), Token("}"))
            select ToDictionary(pairs);

        JsonValue.Rule = Choice(True, False, Null, Number, Quotation, Dictionary, Array);

        Json.Rule = from jsonValue in JsonValue
            from end in EndOfInput
            select jsonValue;
    }

    static IParser<object> Constant(TokenKind kind, object constant)
    {
        return from _ in Token(kind)
            select constant;
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
