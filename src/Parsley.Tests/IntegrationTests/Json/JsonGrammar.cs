using System.Globalization;
using System.Text.RegularExpressions;
using static Parsley.Grammar;

namespace Parsley.Tests.IntegrationTests.Json;

public class JsonGrammar
{
    public static readonly GrammarRule<object> Json = new(nameof(Json));
    static readonly GrammarRule<Token> Whitespace = new(nameof(Whitespace));
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
        Whitespace.Rule =
            Optional(Token(JsonLexer.Whitespace));

        True.Rule =
            Keyword(JsonLexer.@true, true);

        False.Rule =
            Keyword(JsonLexer.@false, false);

        Null.Rule =
            Keyword(JsonLexer.@null, null);

        Number.Rule =
            from number in Token(JsonLexer.Number)
            from trailing in Whitespace
            select (object) decimal.Parse(number.Literal, NumberStyles.Any, CultureInfo.InvariantCulture);

        Quotation.Rule =
            from quotation in Token(JsonLexer.Quotation)
            from trailing in Whitespace
            select Unescape(quotation.Literal);

        Array.Rule =
            from items in Between(Operator(JsonLexer.OpenArray), ZeroOrMore(JsonValue, Operator(JsonLexer.Comma)), Operator(JsonLexer.CloseArray))
            select items.ToArray();

        Pair.Rule =
            from key in Quotation
            from colon in Operator(JsonLexer.Colon)
            from value in JsonValue
            select new KeyValuePair<string, object>(key, value);

        Dictionary.Rule =
            from pairs in Between(Operator(JsonLexer.OpenDictionary), ZeroOrMore(Pair, Operator(JsonLexer.Comma)), Operator(JsonLexer.CloseDictionary))
            select ToDictionary(pairs);

        JsonValue.Rule = Choice(True, False, Null, Number, Quotation, Dictionary, Array);

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
