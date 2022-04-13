using System.Diagnostics.CodeAnalysis;

namespace Parsley;

public static partial class Grammar
{
    /// <summary>
    /// Optional(p) is equivalent to p whenever p succeeds or when p fails after consuming input.
    /// If p fails without consuming input, Optional(p) succeeds.
    /// </summary>
    public static Parser<T?> Optional<T>(Parser<T> parser)
        where T : class
    {
        var nothing = default(T).SucceedWithThisValue();
        return Choice(parser, nothing);
    }

    /// <summary>
    /// Optional(p) is equivalent to p whenever p succeeds or when p fails after consuming input.
    /// If p fails without consuming input, Optional(p) succeeds.
    /// </summary>
    [SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter",
        Justification = "This warning is inaccurate. The other `struct` " +
                        "overload does not in fact hide this one.")]
    public static Parser<T?> Optional<T>(Parser<T> parser, T? ignoredOverloadResolver = null)
        where T : struct
    {
        var nothing = default(T?).SucceedWithThisValue();
        return Choice(parser.Select(x => (T?)x), nothing);
    }
}
