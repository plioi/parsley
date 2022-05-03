using System.Diagnostics.CodeAnalysis;

namespace Parsley;

public static partial class Grammar
{
    /// <summary>
    /// Optional(p) is equivalent to p whenever p succeeds or when p fails after consuming input.
    /// If p fails without consuming input, Optional(p) succeeds.
    /// </summary>
    public static Parser<TItem, TValue?> Optional<TItem, TValue>(Parser<TItem, TValue> parser)
        where TValue : class
    {
        var nothing = default(TValue).SucceedWithThisValue<TItem, TValue?>();
        return Choice(parser, nothing);
    }

    /// <summary>
    /// Optional(p) is equivalent to p whenever p succeeds or when p fails after consuming input.
    /// If p fails without consuming input, Optional(p) succeeds.
    /// </summary>
    [SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter",
        Justification = "This warning is inaccurate. The other `struct` " +
                        "overload does not in fact hide this one.")]
    public static Parser<TItem, TValue?> Optional<TItem, TValue>(Parser<TItem, TValue> parser, TValue? ignoredOverloadResolver = null)
        where TValue : struct
    {
        var nothing = default(TValue?).SucceedWithThisValue<TItem, TValue?>();
        return Choice(parser.Select(x => (TValue?)x), nothing);
    }
}
