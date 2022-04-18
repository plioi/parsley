using System.Globalization;
using static Parsley.Grammar;

namespace Parsley.Tests.IntegrationTests.Json;

public class JsonGrammar
{
    public static readonly Parser<object?> JsonDocument;

    static readonly Parser<object?> Value = default!;
    
    static JsonGrammar()
    {
        var Whitespace = ZeroOrMore(char.IsWhiteSpace);

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

        Value =
            from leading in Whitespace
            from value in Choice(
                from @true in Keyword("true")
                select (object) true,

                from @false in Keyword("false")
                select (object) false,

                from @null in Keyword("null")
                select (object?) null,

                from number in Number
                select (object) decimal.Parse(number, NumberStyles.Any, CultureInfo.InvariantCulture),

                from quotation in Quote
                select quotation,

                from open in Operator("{")
                from pairs in ZeroOrMore(Pair, Operator(","))
                from close in Operator("}")
                select pairs.ToDictionary(x => x.Key, x => x.Value),

                from open in Operator("[")
                from items in ZeroOrMore(Value, Operator(","))
                from close in Operator("]")
                select items.ToArray()
            )
            from trailing in Whitespace
            select value;

        JsonDocument =
            from value in Value
            from end in EndOfInput
            select value;
    }

    static readonly Parser<string> Digits = OneOrMore(char.IsDigit, "0..9");

    static readonly Parser<string> Number =

        from leading in Digits

        from optionalFraction in Optional(
            from dot in Character('.')
            from digits in Digits
            select dot + digits)

        from optionalExponent in Optional(
            from e in Character(x => x is 'e' or 'E', "exponent")
            from sign in Optional(Character(x => x is '+' or '-', "sign").Select(x => x.ToString()))
            from digits in Digits
            select $"{e}{sign}{digits}")

        select $"{leading}{optionalFraction}{optionalExponent}";

    static readonly Parser<char> LetterOrDigit = Character(char.IsLetterOrDigit, "letter or digit");

    static readonly Parser<string> Quote =

        from open in Character('"')
        from content in ZeroOrMore(
            Choice(
                from slash in Character('\\')
                from unescaped in Choice(
                    from escape in Character(c => "\"\\bfnrt/".Contains(c), "escape character")
                    select $"{escape}"
                        .Replace("\"", "\"")
                        .Replace("\\", "\\")
                        .Replace("b", "\b")
                        .Replace("f", "\f")
                        .Replace("n", "\n")
                        .Replace("r", "\r")
                        .Replace("t", "\t")
                        .Replace("/", "/"),

                    from u in Label(Character('u'), "unicode escape sequence")
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
                Character(c => c != '"' && c != '\\', "non-quote, not-slash character").Select(x => x.ToString())
            ))
        from close in Character('"')
        select string.Join("", content);
}
