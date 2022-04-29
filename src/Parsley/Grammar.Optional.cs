using System.Diagnostics.CodeAnalysis;

namespace Parsley;

public static partial class Grammar
{
    /// <summary>
    /// Optional(p) is equivalent to p whenever p succeeds or when p fails after consuming input.
    /// If p fails without consuming input, Optional(p) succeeds.
    /// </summary>
    public static Parser<char, TValue?> Optional<TValue>(Parser<char, TValue> parser)
        where TValue : class
    {
        var nothing = default(TValue).SucceedWithThisValue<char, TValue?>();
        return Choice(
            from x in parser
            select (TValue?)x, nothing);
    }

    /// <summary>
    /// Optional(p) is equivalent to p whenever p succeeds or when p fails after consuming input.
    /// If p fails without consuming input, Optional(p) succeeds.
    /// </summary>
    [SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter",
        Justification = "This warning is inaccurate. The other `struct` " +
                        "overload does not in fact hide this one.")]
    public static Parser<char, TValue?> Optional<TValue>(Parser<char, TValue> parser, TValue? ignoredOverloadResolver = null)
        where TValue : struct
    {
        var nothing = default(TValue?).SucceedWithThisValue<char, TValue?>();
        return Choice(parser.Select(x => (TValue?)x), nothing);
    }
}
