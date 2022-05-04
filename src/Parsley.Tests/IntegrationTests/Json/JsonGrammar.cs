using System.Globalization;
using static Parsley.Grammar;
using static Parsley.Characters;

namespace Parsley.Tests.IntegrationTests.Json;

public class JsonGrammar
{
    record JsonToken(string Kind, object? Value);

    public static readonly Parser<char, object?> JsonDocument;
    static readonly Parser<JsonToken, object?> Value;

    static readonly Parser<char, Void> Whitespace = Skip(IsWhiteSpace);

    static JsonGrammar()
    {
        var True = Token("true");
        var False = Token("false");
        var Null = Token("null");
        var Number = Token("number");
        var Quote = Token("quote");

        Value = Choice(True, False, Null, Number, Quote, Dictionary, Array);

        JsonDocument = (ReadOnlySpan<char> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var tokens = Tokenizer()(input, ref index, out succeeded, out expectation);

            if (succeeded)
            {
                var tokenIndex = 0;
                return Value(tokens!.ToArray(), ref tokenIndex, out succeeded, out expectation);
            }

            return null;
        };
    }

    static Parser<char, IReadOnlyList<JsonToken>> Tokenizer() =>
        from leading in Whitespace
        from tokens in
            ZeroOrMore(
                from token in Choice(
                    Literal("true", true),
                    Literal("false", false),
                    Literal("null", null),
                    Symbol("["),
                    Symbol("]"),
                    Symbol("{"),
                    Symbol("}"),
                    Symbol(","),
                    Symbol(":"),
                    NumberLiteral,
                    StringLiteral
                )
                from trailing in Whitespace
                select token)
        select tokens;

    static Parser<char, JsonToken> Literal(string literal, object? value) =>
        from x in Keyword(literal)
        select new JsonToken(literal, value);

    static Parser<char, JsonToken> Symbol(string literal) =>
        from x in Operator(literal)
        select new JsonToken(literal, literal);

    static Parser<JsonToken, object> Token(string kind) =>
        from x in Single<JsonToken>(x => x.Kind == kind, kind)
        select x.Value;

    static Parser<JsonToken, object> Array =>
        from open in Token("[")
        from items in ZeroOrMore(Value, Token(","))
        from close in Token("]")
        select items.ToArray();

    static Parser<JsonToken, object> Dictionary
    {
        get
        {
            var Pair =
                from key in Token("quote")
                from colon in Token(":")
                from value in Value
                select new KeyValuePair<string, object>((string) key, value);

            return
                from open in Token("{")
                from pairs in ZeroOrMore(Pair, Token(","))
                from close in Token("}")
                select pairs.ToDictionary(x => x.Key, x => x.Value);
        }
    }

    static Parser<char, JsonToken> NumberLiteral
    {
        get
        {
            var Digits = OneOrMore(IsDigit, "0..9");

            return from leading in Digits

                from optionalFraction in Optional(
                    from dot in Single('.')
                    from digits in Digits
                    select dot + digits)

                from optionalExponent in Optional(
                    from e in Single<char>(x => x is 'e' or 'E', "exponent")
                    from sign in Optional(Single<char>(x => x is '+' or '-', "sign").Select(x => x.ToString()))
                    from digits in Digits
                    select $"{e}{sign}{digits}")

                from value in Evaluate($"{leading}{optionalFraction}{optionalExponent}")
                select new JsonToken("number", value);
        }
    }

    static Parser<char, decimal> Evaluate(string candidate)
    {
        return (ReadOnlySpan<char> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            succeeded = decimal.TryParse(candidate, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value);

            expectation = succeeded ? null : "decimal within valid range";

            return value;
        };
    }

    static Parser<char, JsonToken> StringLiteral
    {
        get
        {
            var LetterOrDigit = Single(IsLetterOrDigit, "letter or digit");

            return
                from open in Single('"')
                from content in ZeroOrMore(
                    Choice(
                        from slash in Single('\\')
                        from unescaped in Choice(
                            from escape in Single<char>(c => "\"\\bfnrt/".Contains(c), "escape character")
                            select $"{escape}"
                                .Replace("\"", "\"")
                                .Replace("\\", "\\")
                                .Replace("b", "\b")
                                .Replace("f", "\f")
                                .Replace("n", "\n")
                                .Replace("r", "\r")
                                .Replace("t", "\t")
                                .Replace("/", "/"),

                            from u in Label(Single('u'), "unicode escape sequence")
                            from unicodeDigits in Repeat(LetterOrDigit, 4)
                            select char.ConvertFromUtf32(
                                int.Parse(
                                    new string(unicodeDigits),
                                    NumberStyles.HexNumber,
                                    CultureInfo.InvariantCulture))
                        )
                        select unescaped,
                        Single<char>(c => c != '"' && c != '\\', "non-quote, not-slash character").Select(x => x.ToString())
                    ))
                from close in Single('"')
                select new JsonToken("quote", string.Join("", content));
        }
    }
}
