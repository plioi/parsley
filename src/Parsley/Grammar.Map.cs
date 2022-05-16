namespace Parsley;

partial class Grammar
{
    public static Parser<TItem, TValue> Map<TItem, T1, TValue>(
        Parser<TItem, T1> parser1,
        Func<T1, TValue> combineIntermediateResults)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var value1 = parser1(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            expectation = null;
            succeeded = true;

            return combineIntermediateResults(value1!);
        };
    }

    public static Parser<TItem, TValue> Map<TItem, T1, T2, TValue>(
        Parser<TItem, T1> parser1,
        Parser<TItem, T2> parser2,
        Func<T1, T2, TValue> combineIntermediateResults)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var value1 = parser1(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            var value2 = parser2(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            expectation = null;
            succeeded = true;

            return combineIntermediateResults(value1!, value2!);
        };
    }

    public static Parser<TItem, TValue> Map<TItem, T1, T2, T3, TValue>(
        Parser<TItem, T1> parser1,
        Parser<TItem, T2> parser2,
        Parser<TItem, T3> parser3,
        Func<T1, T2, T3, TValue> combineIntermediateResults)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var value1 = parser1(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            var value2 = parser2(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            var value3 = parser3(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            expectation = null;
            succeeded = true;

            return combineIntermediateResults(value1!, value2!, value3!);
        };
    }

    public static Parser<TItem, TValue> Map<TItem, T1, T2, T3, T4, TValue>(
        Parser<TItem, T1> parser1,
        Parser<TItem, T2> parser2,
        Parser<TItem, T3> parser3,
        Parser<TItem, T4> parser4,
        Func<T1, T2, T3, T4, TValue> combineIntermediateResults)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var value1 = parser1(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            var value2 = parser2(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            var value3 = parser3(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            var value4 = parser4(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            expectation = null;
            succeeded = true;

            return combineIntermediateResults(value1!, value2!, value3!, value4!);
        };
    }

    public static Parser<TItem, TValue> Map<TItem, T1, T2, T3, T4, T5, TValue>(
        Parser<TItem, T1> parser1,
        Parser<TItem, T2> parser2,
        Parser<TItem, T3> parser3,
        Parser<TItem, T4> parser4,
        Parser<TItem, T5> parser5,
        Func<T1, T2, T3, T4, T5, TValue> combineIntermediateResults)
    {
        return (ReadOnlySpan<TItem> input, ref int index, out bool succeeded, out string? expectation) =>
        {
            var value1 = parser1(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            var value2 = parser2(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            var value3 = parser3(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            var value4 = parser4(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            var value5 = parser5(input, ref index, out succeeded, out expectation);

            if (!succeeded)
            {
                succeeded = false;
                return default;
            }

            expectation = null;
            succeeded = true;

            return combineIntermediateResults(value1!, value2!, value3!, value4!, value5!);
        };
    }
}
