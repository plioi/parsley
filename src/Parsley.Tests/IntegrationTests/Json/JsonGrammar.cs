using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Parsley.Tests.IntegrationTests.Json
{
    public class JsonGrammar : Grammar
    {
        public static readonly GrammarRule<object> Json = new GrammarRule<object>();
        static readonly GrammarRule<object> True = new GrammarRule<object>();
        static readonly GrammarRule<object> False = new GrammarRule<object>();
        static readonly GrammarRule<object> Null = new GrammarRule<object>();
        static readonly GrammarRule<object> Number = new GrammarRule<object>();
        static readonly GrammarRule<string> Quotation = new GrammarRule<string>();
        static readonly GrammarRule<IList<object>> Array = new GrammarRule<IList<object>>();
        static readonly GrammarRule<KeyValuePair<string, object>> Pair = new GrammarRule<KeyValuePair<string, object>>();
        static readonly GrammarRule<Dictionary<string, object>> Dictionary = new GrammarRule<Dictionary<string, object>>();
        static readonly GrammarRule<object> JsonValue = new GrammarRule<object>();

        static JsonGrammar()
        {
            True.Rule =
                Constant<object>(JsonLexer.True, true);

            False.Rule =
                Constant<object>(JsonLexer.False, false);

            Null.Rule =
                Constant<object>(JsonLexer.Null, null);

            Number.Rule =
                Token(JsonLexer.Number, l => (object)decimal.Parse(l, NumberStyles.Any));

            Quotation.Rule =
                Token(JsonLexer.Quotation, Unescape);

            Array.Rule =
                Between(TokenG(JsonLexer.OpenArray), ZeroOrMore(JsonValue, TokenG(JsonLexer.Comma)), TokenG(JsonLexer.CloseArray));

            Pair.Rule =
                NameValuePair(Quotation, TokenG(JsonLexer.Colon), JsonValue);

            Dictionary.Rule =
                Between(TokenG(JsonLexer.OpenDictionary), ZeroOrMore(Pair, TokenG(JsonLexer.Comma)), TokenG(JsonLexer.CloseDictionary))
                .Bind(ToDictionary);

            JsonValue.Rule = Choice(True, False, Null, Number, Quotation, Dictionary, Array);

            Json.Rule = OccupiesEntireInput(JsonValue);
        }

        static string Unescape(string quotation)
        {
            string result = quotation.Substring(1, quotation.Length - 2); //Remove leading and trailing quotation marks

            result = Regex.Replace(result, @"\\u[0-9a-fA-F]{4}",
                                   match => char.ConvertFromUtf32(int.Parse(match.Value.Replace("\\u", ""), NumberStyles.HexNumber)));

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
