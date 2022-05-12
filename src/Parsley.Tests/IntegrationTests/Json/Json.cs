using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using static Parsley.Grammar;
using static Parsley.Characters;

namespace Parsley.Tests.IntegrationTests.Json;

public class Json
{
    public static bool TryParse(
        ReadOnlySpan<char> input,
        [NotNullWhen(true)] out object? value,
        [NotNullWhen(false)] out ParseError? error)
    {
        if (Tokenize.TryParse(input, out var tokens, out error))
        {
            if (Value.TryParse(tokens.ToArray(), out value, out error))
                return true;

            error = new ParseError(tokens[error.Index].Index, error.Expectation);
        }

        value = null;
        return false;
    }

    record JsonToken(string Kind, object? Value, int Index);

    static readonly Parser<char, Void> Whitespace = Skip(IsWhiteSpace);

    static Parser<char, IReadOnlyList<JsonToken>> Tokenize =>
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

    static readonly Parser<JsonToken, object?> Value =
        Recursive(() => Choice(
            Token("true"),
            Token("false"),
            Token("null"),
            Token("number"),
            Token("string"),
            Dictionary,
            Array));

    static Parser<char, JsonToken> Literal(string literal, object? value) =>
        from index in Index<char>()
        from x in Keyword(literal)
        select new JsonToken(literal, value, index);

    static Parser<char, JsonToken> Symbol(string literal) =>
        from index in Index<char>()
        from x in Operator(literal)
        select new JsonToken(literal, literal, index);

    static Parser<JsonToken, object> Token(string kind) =>
        from x in Single<JsonToken>(x => x.Kind == kind, kind)
        select x.Value;

    static Parser<JsonToken, object> Array =>
        List(Value, "[", "]").Select(values => values.ToArray());

    static Parser<JsonToken, object> Dictionary =>
        List(Pair, "{", "}").Select(pairs => pairs.ToDictionary(x => (string)x.key, x => x.value));

    static Parser<JsonToken, (object key, object? value)> Pair =>
        from key in Token("string")
        from colon in Token(":")
        from value in Value
        select (key, value);

    static Parser<JsonToken, IReadOnlyList<TValue>> List<TValue>(Parser<JsonToken, TValue> item, string open, string close) =>
        from openToken in Token(open)
        from items in ZeroOrMore(item, Token(","))
        from closeToken in Token(close)
        select items;

    static Parser<char, JsonToken> NumberLiteral
    {
        get
        {
            var Digits = OneOrMore(IsDigit, "0..9");

            return
                from index in Index<char>()

                from leading in Digits

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
                select new JsonToken("number", value, index);
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
            var escapeCharacter =
                from escape in Single<char>(c => "\"\\bfnrt/".Contains(c), "escape character")
                select (escape switch
                {
                    'b' => '\b',
                    'f' => '\f',
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    _ => escape
                }).ToString();

            var unicodeEscapeCharacters =
                from u in Label(Single('u'), "unicode escape sequence")
                from unicodeDigits in Repeat(Single(IsLetterOrDigit, "letter or digit"), 4)
                select char.ConvertFromUtf32(
                    int.Parse(
                        new string(unicodeDigits),
                        NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture));

            var charactersFromEscapeSequence =
                from slash in Single('\\')
                from unescaped in Choice(escapeCharacter, unicodeEscapeCharacters)
                select unescaped;

            var literalCharacters =
                OneOrMore(c => c != '"' && c != '\\', "non-quote, not-slash character");

            return
                from index in Index<char>()
                from open in Single('"')
                from content in ZeroOrMore(Choice(charactersFromEscapeSequence, literalCharacters))
                from close in Single('"')
                select new JsonToken("string", string.Join("", content), index);
        }
    }
}
