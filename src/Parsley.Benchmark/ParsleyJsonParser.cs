using System.Globalization;
using System.Text.RegularExpressions;

namespace Parsley.Benchmark;

public static class ParsleyJsonParser
{
    class JsonLexer : Lexer
    {
        public JsonLexer()
            : base(Whitespace,
                   @null, @true, @false,
                   Comma, OpenArray, CloseArray, OpenDictionary, CloseDictionary, Colon,
                   Number, Quotation) { }

        static readonly TokenKind Whitespace = new Pattern("whitespace", @"\s+", skippable: true);
        
        public static readonly Keyword @null = new("null");
        public static readonly Keyword @true = new("true");
        public static readonly Keyword @false = new("false");
        static readonly Operator Comma = new(",");
        static readonly Operator OpenArray = new("[");
        static readonly Operator CloseArray = new("]");
        static readonly Operator OpenDictionary = new("{");
        static readonly Operator CloseDictionary = new("}");
        static readonly Operator Colon = new(":");

        public static readonly TokenKind Quotation = new Pattern("string", @"
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

        public static readonly TokenKind Number = new Pattern("number", @"
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
    }

    class JsonGrammar : Grammar
    {
        public static readonly GrammarRule<object?> Json = new();
        static readonly GrammarRule<object?> JsonValue = new();
        static readonly GrammarRule<object?> True = new();
        static readonly GrammarRule<object?> False = new();
        static readonly GrammarRule<object?> Null = new();
        static readonly GrammarRule<object?> Number = new();
        static readonly GrammarRule<string?> Quotation = new();
        static readonly GrammarRule<object[]> Array = new();
        static readonly GrammarRule<KeyValuePair<string, object?>> Pair = new();
        static readonly GrammarRule<Dictionary<string, object?>> Dictionary = new();

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
                select new Dictionary<string, object>(pairs);

            JsonValue.Rule = Choice(True, False, Null, Number, Quotation, Dictionary, Array);

            Json.Rule = from jsonValue in JsonValue
                        from end in EndOfInput
                        select jsonValue;
        }

        static IParser<object?> Constant(TokenKind kind, object? constant)
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
    }

    static IEnumerable<Token> Tokenize(string input) => new JsonLexer().Tokenize(input);
    
    public static object? Parse(string input)
    {
        var reply = JsonGrammar.Json.Parse(new TokenStream(Tokenize(input)));

        if (reply.Success)
            return reply.Value;

        throw new Exception(reply.ErrorMessages.ToString());
    }
}
