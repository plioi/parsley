using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Parsley.IntegrationTests.Json
{
    public class JsonGrammar : Grammar
    {
        public static readonly GrammarRule<object> Json = new GrammarRule<object>();
        private static readonly GrammarRule<object> True = new GrammarRule<object>();
        private static readonly GrammarRule<object> False = new GrammarRule<object>(); 
        private static readonly GrammarRule<object> Null = new GrammarRule<object>();
        private static readonly GrammarRule<object> Number = new GrammarRule<object>();
        private static readonly GrammarRule<string> Quotation = new GrammarRule<string>();
        private static readonly GrammarRule<object[]> Array = new GrammarRule<object[]>();
        private static readonly GrammarRule<KeyValuePair<string, object>> Pair = new GrammarRule<KeyValuePair<string, object>>();
        private static readonly GrammarRule<Dictionary<string, object>> Dictionary = new GrammarRule<Dictionary<string, object>>();

        static JsonGrammar()
        {
            True.Rule =
                Constant(JsonTokenStream.@true, true);

            False.Rule =
                Constant(JsonTokenStream.@false, false);

            Null.Rule =
                Constant(JsonTokenStream.@null, null);

            Number.Rule =
                from number in Token(JsonTokenStream.Number)
                select (object) Decimal.Parse(number.Literal, NumberStyles.Any);

            Quotation.Rule =
                from quotation in Token(JsonTokenStream.Quotation)
                select Unescape(quotation.Literal);

            Array.Rule =
                from items in Between(Token("["), ZeroOrMore(Json, Token(",")), Token("]"))
                select items.ToArray();

            Pair.Rule =
                from key in Quotation
                from colon in Token(":")
                from value in Json
                select new KeyValuePair<string, object>(key, value);

            Dictionary.Rule =
                from pairs in Between(Token("{"), ZeroOrMore(Pair, Token(",")), Token("}"))
                select ToDictionary(pairs);

            Json.Rule =
                Choice(True, False, Null, Number, Quotation, Dictionary, Array);
        }

        private static Parser<object> Constant(TokenKind kind, object constant)
        {
            return from _ in Token(kind)
                   select constant;
        }

        private static string Unescape(string quotation)
        {
            string result = quotation.Substring(1, quotation.Length - 2); //Remove leading and trailing quotation marks

            result = Regex.Replace(result, @"\\u[0-9a-fA-F]{4}",
                                   match => Char.ConvertFromUtf32(int.Parse(match.Value.Replace("\\u", ""), NumberStyles.HexNumber)));

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

        private static Dictionary<string, object> ToDictionary(IEnumerable<KeyValuePair<string, object>> pairs)
        {
            var result = new Dictionary<string, object>();

            foreach (var pair in pairs)
                result[pair.Key] = pair.Value;

            return result;
        }
    }
}
