using static Parsley.Grammar;
using static Parsley.Characters;

namespace Parsley.Benchmark;

public static class ParsleyJsonParser
{
    static Parser<char, object?> Literal(string literal, object? value)
        => Map(Keyword(literal), _ => value);

    static readonly Parsley.Parser<char, Void> SkipWhitespaces = Skip(IsWhiteSpace);

    static readonly Parsley.Parser<char, string> String =
        Between(Single('"'), ZeroOrMore(c => c != '"'), Single('"'));

    static readonly Parsley.Parser<char, object?> Json =
        Recursive(() =>
        {
            var @true = Literal("true", true);
            var @false = Literal("false", false);
            var @null = Literal("null", null);
            var @int = Map(OneOrMore(IsDigit, "digit"), digits => (object?) int.Parse(digits));

            return Choice(@true, @false, @null, @int, String, Array, Object);
        });

    static Parsley.Parser<char, object> Array =>
        List('[', Json, ']', items => items.ToArray());

    static Parsley.Parser<char, object> Object
    {
        get
        {
            var ColonWhitespace =
                Between(SkipWhitespaces, Single(':'), SkipWhitespaces);

            var Pair =
                Map(String, ColonWhitespace, Json,
                    (name, _, val) => new KeyValuePair<string, object?>(name, val));

            return List('{', Pair, '}',
                pairs => new Dictionary<string, object?>(pairs));
        }
    }

    static Parser<char, TValue> List<TItem, TValue>(
        char open,
        Parsley.Parser<char, TItem> item,
        char close,
        Func<IReadOnlyList<TItem>, TValue> convert) =>
        Map(
            Between(
                Single(open),
                ZeroOrMore(Between(SkipWhitespaces, item, SkipWhitespaces), Single(',')),
                Single(close)),
            convert);

    public static object? Parse(string input)
    {
        if (Json.TryParse(input, out object? value, out var error))
            return value;

        throw new Exception(error.Expectation + " expected");
    }
}
