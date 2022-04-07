namespace Parsley.Tests.IntegrationTests.Json
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class JsonGrammar : Grammar
    {
        public static readonly GrammarRule<object> Json = new();
        static readonly GrammarRule<object> JsonValue = new();
        static readonly GrammarRule<object> True = new();
        static readonly GrammarRule<object> False = new();
        static readonly GrammarRule<object> Null = new();
        static readonly GrammarRule<object> Number = new();
        static readonly GrammarRule<string> Quotation = new();
        static readonly GrammarRule<object[]> Array = new();
        static readonly GrammarRule<KeyValuePair<string, object>> Pair = new();
        static readonly GrammarRule<Dictionary<string, object>> Dictionary = new();

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
                                   match => Char.ConvertFromUtf32(int.Parse(match.Value.Replace("\\u", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture)));

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
}
