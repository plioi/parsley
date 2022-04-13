#nullable disable
namespace Parsley;

public static partial class Grammar
{
    /// <summary>
    /// Optional(p) is equivalent to p whenever p succeeds or when p fails after consuming input.
    /// If p fails without consuming input, Optional(p) succeeds.
    /// </summary>
    public static Parser<T> Optional<T>(Parser<T> parser)
    {
        var nothing = default(T).SucceedWithThisValue();
        return Choice(parser, nothing);
    }
}
