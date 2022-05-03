using System.Globalization;
using static Parsley.Grammar;
using static Parsley.Characters;

namespace Parsley.Tests.IntegrationTests.Json;

public class JsonGrammar
{
    public static readonly Parser<char, object?> Json;

    static readonly Parser<char, Void> Whitespace = Skip(IsWhiteSpace);

    static JsonGrammar()
    {
        var True = Literal("true", true);
        var False = Literal("false", false);
        var Null = Literal("null", null);

        Json =
            from leading in Whitespace
            from value in Choice(True, False, Null, Number, Quote, Dictionary, Array)
            from trailing in Whitespace
            select value;
    }

    static Parser<char, object> Literal(string literal, object? value) =>
        from x in Keyword(literal)
        select value;

    static Parser<char, object> Array =>
        from open in Operator("[")
        from items in ZeroOrMore(Json, Operator(","))
        from close in Operator("]")
        select items.ToArray();

    static Parser<char, object> Dictionary
    {
        get
        {
            var Key =
                from leading in Whitespace
                from quote in Quote
                from trailing in Whitespace
                select quote;

            var Pair =
                from key in Key
                from colon in Operator(":")
                from value in Json
                select new KeyValuePair<string, object>(key, value);

            return
                from open in Operator("{")
                from pairs in ZeroOrMore(Pair, Operator(","))
                from close in Operator("}")
                select pairs.ToDictionary(x => x.Key, x => x.Value);
        }
    }

    static Parser<char, object> Number
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
                select (object) value;
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

    static Parser<char, string> Quote
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
                select string.Join("", content);
        }
    }
}
