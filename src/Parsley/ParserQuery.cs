namespace Parsley
{
    using System;

    public static class ParserQuery
    {
        /// <summary>
        /// Converts any value into a parser that always succeeds with the given value in its reply.
        /// </summary>
        /// <remarks>
        /// In monadic terms, this is the 'Unit' function.
        /// </remarks>
        /// <typeparam name="T">The type of the value to treat as a parse result.</typeparam>
        /// <param name="value">The value to treat as a parse result.</param>
        public static Parser<T> SucceedWithThisValue<T>(this T value)
        {
            return new LambdaParser<T>(tokens => new Parsed<T>(value, tokens));
        }

        /// <summary>
        /// Allows LINQ syntax to construct a new parser from a simpler parser, using a single 'from' clause.
        /// </summary>
        public static Parser<U> Select<T, U>(this Parser<T> parser, Func<T, U> constructResult)
        {
            return parser.Bind(t => constructResult(t).SucceedWithThisValue());
        }

        /// <summary>
        /// Allows LINQ syntax to contruct a new parser from an ordered sequence of simpler parsers, using multiple 'from' clauses.
        /// </summary>
        public static Parser<V> SelectMany<T, U, V>(this Parser<T> parser, Func<T, Parser<U>> k, Func<T, U, V> s)
        {
            return parser.Bind(x => k(x).Bind(y => s(x, y).SucceedWithThisValue()));
        }

        /// <summary>
        /// Extend a parser such that, after executing, the remaining input is processed by the next parser in the chain.
        /// </summary>
        /// <remarks>
        /// In monadic terms, this is the 'Bind' function.
        /// </remarks>
        private static Parser<U> Bind<T, U>(this Parser<T> parser, Func<T, Parser<U>> constructNextParser)
        {
            return new LambdaParser<U>(tokens =>
            {
                var reply = parser.Parse(tokens);

                if (reply.Success)
                    return constructNextParser(reply.Value).Parse(reply.UnparsedTokens);

                return new Error<U>(reply.UnparsedTokens, reply.ErrorMessages);
            });
        }
    }
}