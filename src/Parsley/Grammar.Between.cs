namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// Between(left, goal, right) parses its arguments in order.  If all three
    /// parsers succeed, the result of the goal parser is returned.
    /// </summary>
    public static Parser<TItem, TGoal> Between<TItem, TLeft, TGoal, TRight>(
        Parser<TItem, TLeft> left,
        Parser<TItem, TGoal> goal,
        Parser<TItem, TRight> right)
        => Map(left, goal, right, (_, g, _) => g);
}
