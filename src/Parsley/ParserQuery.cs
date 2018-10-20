using System;

namespace Parsley
{
    public static class ParserQuery
    {
        /// <summary>
        /// Allows LINQ syntax to construct a new parser from a simpler parser, using a single 'from' clause.
        /// </summary>
        public static IParser<TResult> Select<TInterim, TResult>(this IParser<TInterim> parser, Func<TInterim, TResult> resultContinuation)
        {
            return new MonadicBindParser<TInterim, TResult>(parser, resultContinuation);
        }

        /// <summary>
        /// Allows LINQ syntax to contruct a new parser from an ordered sequence of simpler parsers, using multiple 'from' clauses.
        /// </summary>
        public static IParser<TResult> SelectMany<T1, T2, TResult>(this IParser<T1> parser, Func<T1, IParser<T2>> value1ToParser2Continuation, Func<T1, T2, TResult> resultContinuation)
        {
            return new MonadicBindParser<T1, T2, TResult>(parser, value1ToParser2Continuation, resultContinuation);
        }
    }
}