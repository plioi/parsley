using System.Globalization;
using static Parsley.Grammar;

namespace Parsley.Tests.IntegrationTests.Json;

public class JsonGrammar
{
    public static readonly Parser<char, object?> JsonDocument;

    static readonly Parser<char, string> Whitespace = ZeroOrMore(char.IsWhiteSpace);
    static readonly Parser<char, object?> Value = default!;

    static JsonGrammar()
    {
        var True = Literal("true", true);
        var False = Literal("false", false);
        var Null = Literal("null", null);

        Value =
            from leading in Whitespace
            from value in Choice(
                True,
                False,
                Null,
                Number,
                from quotation in Quote select (object) quotation,
                Dictionary,
                Array
            )
            from trailing in Whitespace
            select value;

        JsonDocument =
            from value in Value
            from end in EndOfInput()
            select value;
    }

    static Parser<char, object> Literal(string literal, object? value) =>
        from x in Keyword(literal)
        select value;

    static Parser<char, object> Array =>
        from open in Operator("[")
        from items in ZeroOrMore(Value, Operator(","))
        from close in Operator("]")
        select (object) items.ToArray();

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
                from value in Value
                select new KeyValuePair<string, object>(key, value);

            return
                from open in Operator("{")
                from pairs in ZeroOrMore(Pair, Operator(","))
                from close in Operator("}")
                select (object) pairs.ToDictionary(x => x.Key, x => x.Value);
        }
    }

    static Parser<char, object> Number
    {
        get
        {
            var Digits = OneOrMore(char.IsDigit, "0..9");

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

                select (object) decimal.Parse(
                    $"{leading}{optionalFraction}{optionalExponent}",
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture);
        }
    }

    static Parser<char, string> Quote
    {
        get
        {
            var LetterOrDigit = Single<char>(char.IsLetterOrDigit, "letter or digit");

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
                            from _0 in LetterOrDigit
                            from _1 in LetterOrDigit
                            from _2 in LetterOrDigit
                            from _3 in LetterOrDigit
                            select char.ConvertFromUtf32(
                                int.Parse($"{_0}{_1}{_2}{_3}",
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
