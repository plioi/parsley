using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Parsley.Test.IntegrationTests.Json
{
    public class JsonGrammar : Grammar
    {
        public static Parser<object> JSON
        {
            get
            {
                return Choice(True, False, Null, Number, Quotation, Dictionary, Array);
            }
        }

        private static Parser<object> True
        {
            get
            {
                return Constant(JsonLexer.@true, true);
            }
        }

        private static Parser<object> False
        {
            get
            {
                return Constant(JsonLexer.@false, false);
            }
        }

        private static Parser<object> Null
        {
            get
            {
                return Constant(JsonLexer.@null, null);
            }
        }

        private static Parser<object> Number
        {
            get
            {
                return from number in Token(JsonLexer.Number)
                       select (object)Decimal.Parse(number.Literal, NumberStyles.Any);
            }
        }

        private static Parser<string> Quotation
        {
            get
            {
                return from quotation in Token(JsonLexer.Quotation)
                       select Unescape(quotation.Literal);
            }
        }

        private static Parser<object[]> Array
        {
            get
            {
                //Delay evaluation to avoid infinite recursion!
                Parser<object> lazyValue = tokens => JSON(tokens);

                return from items in Between(Token("["), ZeroOrMore(lazyValue, Token(",")), Token("]"))
                       select items.ToArray();
            }
        }

        private static Parser<Dictionary<string, object>> Dictionary
        {
            get
            {
                return from pairs in Between(Token("{"), ZeroOrMore(Pair, Token(",")), Token("}"))
                       select ToDictionary(pairs);
            }
        }

        private static Parser<KeyValuePair<string, object>> Pair
        {
            get
            {
                //Delay evaluation to avoid infinite recursion!
                Parser<object> lazyValue = tokens => JSON(tokens);

                return from key in Quotation
                       from colon in Token(":")
                       from value in lazyValue
                       select new KeyValuePair<string, object>(key, value);
            }
        }

        #region Helpers

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

        #endregion
    }
}
