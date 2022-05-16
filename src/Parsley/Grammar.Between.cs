namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// Between(left, goal, right) parses its arguments in order.  If all three
    /// parsers succeed, the result of the goal parser is returned.
    ///
    /// <para>Between is an optimized version of the query:</para>
    ///
    /// <code>
    ///     from L in left
    ///     from G in goal
    ///     from R in right
    ///     select G
    /// </code>
    /// </summary>
    public static Parser<TItem, TGoal> Between<TItem, TLeft, TGoal, TRight>(
        Parser<TItem, TLeft> left,
        Parser<TItem, TGoal> goal,
        Parser<TItem, TRight> right)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            left(input, ref index, out succeeded, out expectation);

            if (succeeded)
            {
                var goalValue = goal(input, ref index, out succeeded, out expectation);

                if (succeeded)
                {
                    right(input, ref index, out succeeded, out expectation);

                    if (succeeded)
                    {
                        expectation = null;
                        succeeded = true;
                        return goalValue;
                    }
                }
            }

            succeeded = false;
            return default;
        };
    }
}
