using System;
using System.Collections.Generic;
using System.Linq;

namespace Parsley
{
    public abstract class Grammar
    {
        public static Parser<T> Fail<T>()
        {
            return tokens => new Error<T>(tokens, ErrorMessage.Unknown());
        }

        public static Parser<Token> EndOfInput
        {
            get { return Token(Lexer.EndOfInput); }
        }

        public static Parser<Token> Token(TokenKind kind)
        {
            return tokens =>
            {
                if (tokens.CurrentToken.Kind == kind)
                    return new Parsed<Token>(tokens.CurrentToken, tokens.Advance());

                return new Error<Token>(tokens, ErrorMessage.Expected(kind.Name));
            };
        }

        public static Parser<Token> Token(string expectation)
        {
            return tokens =>
            {
                if (tokens.CurrentToken.Literal == expectation)
                    return new Parsed<Token>(tokens.CurrentToken, tokens.Advance());

                return new Error<Token>(tokens, ErrorMessage.Expected(expectation));
            };
        }

        /// <summary>
        /// ZeroOrMore(p) repeatedly applies an parser p until it fails, returing
        /// the list of values returned by successful applications of p.  At the
        /// end of the sequence, p must fail without consuming input, otherwise the
        /// sequence will fail with the error reported by p.
        /// </summary>
        public static Parser<IEnumerable<T>> ZeroOrMore<T>(Parser<T> item)
        {
            return tokens =>
            {
                Position oldPosition = tokens.Position;
                var reply = item(tokens);
                Position newPosition = reply.UnparsedTokens.Position;

                var list = new List<T>();

                while (reply.Success)
                {
                    if (oldPosition == newPosition)
                        throw new Exception("Parser encountered a potential infinite loop.");

                    list.Add(reply.Value);
                    oldPosition = newPosition;
                    reply = item(reply.UnparsedTokens);
                    newPosition = reply.UnparsedTokens.Position;
                }

                //The item parse finally failed.

                if (oldPosition != newPosition)
                    return new Error<IEnumerable<T>>(reply.UnparsedTokens, reply.ErrorMessages);

                return new Parsed<IEnumerable<T>>(list, reply.UnparsedTokens, reply.ErrorMessages);
            };
        }

        /// <summary>
        /// OneOrMore(p) behaves like ZeroOrMore(p), except that p must succeed at least one time.
        /// </summary>
        public static Parser<IEnumerable<T>> OneOrMore<T>(Parser<T> item)
        {
            return from first in item
                   from rest in ZeroOrMore(item)
                   select List(first, rest);
        }

        /// <summary>
        /// ZeroOrMore(p, s) parses zero or more occurrences of p separated by occurrences of s,
        /// returning the list of values returned by successful applications of p.
        /// </summary>
        public static Parser<IEnumerable<T>> ZeroOrMore<T, S>(Parser<T> item, Parser<S> separator)
        {
            return Choice(OneOrMore(item, separator), Zero<T>());
        }

        /// <summary>
        /// OneOrMore(p, s) behaves like ZeroOrMore(p, s), except that p must succeed at least one time.
        /// </summary>
        public static Parser<IEnumerable<T>> OneOrMore<T, S>(Parser<T> item, Parser<S> separator)
        {
            return from first in item
                   from rest in ZeroOrMore(from sep in separator
                                           from next in item
                                           select next)
                   select List(first, rest);
        }

        public static Parser<TAccumulated> LeftAssociative<TAccumulated, TSeparator>(Parser<TAccumulated> item, Parser<TSeparator> separator, Func<TAccumulated, Tuple<TSeparator, TAccumulated>, TAccumulated> associatePair)
        {
            return from first in item
                   from pairs in ZeroOrMore(from s in separator
                                            from i in item
                                            select Tuple.Create(s, i))
                   select pairs.Aggregate(first, associatePair);
        }

        /// <summary>
        /// Between(left, goal, right) parses its arguments in order.  If all three
        /// parsers succeed, the result of the goal parser is returned.
        /// </summary>
        public static Parser<TGoal> Between<TLeft, TGoal, TRight>(Parser<TLeft> left, 
                                                                  Parser<TGoal> goal, 
                                                                  Parser<TRight> right)
        {
            return from L in left
                   from G in goal
                   from R in right
                   select G;
        }

        /// <summary>
        /// Optional(p) is equivalent to p whenever p succeeds or when p fails after consuming input.
        /// If p fails without consuming input, Optional(p) succeeds.
        /// </summary>
        public static Parser<T> Optional<T>(Parser<T> parse)
        {
            var nothing = default(T).SucceedWithThisValue();
            return Choice(parse, nothing);
        }

        /// <summary>
        /// The parser Attempt(p) behaves like parser p, except that it pretends
        /// that it hasn't consumed any input when an error occurs. This combinator
        /// is used whenever arbitrary look ahead is needed.
        /// </summary>
        public static Parser<T> Attempt<T>(Parser<T> parse)
        {
            return tokens =>
            {
                var start = tokens.Position;
                var reply = parse(tokens);
                var newPosition = reply.UnparsedTokens.Position;

                if (reply.Success || start == newPosition)
                    return reply;

                return new Error<T>(tokens, ErrorMessage.Backtrack(newPosition, reply.ErrorMessages));
            };
        }

        /// <summary>
        /// Choice() always fails without consuming input.
        /// 
        /// Choice(p) is equivalent to p.
        /// 
        /// For 2 or more inputs, parsers are applied from left
        /// to right.  If a parser succeeds, its reply is returned.
        /// If a parser fails without consuming input, the next parser
        /// is attempted.  If a parser fails after consuming input,
        /// subsequent parsers will not be attempted.  As long as
        /// parsers conume no input, their error messages are merged.
        ///
        /// Choice is 'predictive' since p[n+1] is only tried when
        /// p[n] didn't consume any input (i.e. the look-ahead is 1).
        /// This non-backtracking behaviour allows for both an efficient
        /// implementation of the parser combinators and the generation
        /// of good error messages.
        /// </summary>
        public static Parser<T> Choice<T>(params Parser<T>[] parsers)
        {
            if (parsers.Length == 0)
                return Fail<T>();

            return tokens =>
            {
                var start = tokens.Position;
                var reply = parsers[0](tokens);
                var newPosition = reply.UnparsedTokens.Position;

                var errors = ErrorMessageList.Empty;
                var i = 1;
                while (!reply.Success && (start == newPosition) && i < parsers.Length)
                {
                    errors = errors.Merge(reply.ErrorMessages);
                    reply = parsers[i](tokens);
                    newPosition = reply.UnparsedTokens.Position;
                    i++;
                }
                if (start == newPosition)
                {
                    errors = errors.Merge(reply.ErrorMessages);
                    if (reply.Success)
                        reply = new Parsed<T>(reply.Value, reply.UnparsedTokens, errors);
                    else
                        reply = new Error<T>(reply.UnparsedTokens, errors);
                }

                return reply;
            };
        }

        /// <summary>
        /// When parser p consumes any input, Label(p, e) is the same as p.
        /// When parser p does not consume any input, Label(p, e) is the same
        /// as p, except any messages are replaced with expectation e.
        /// </summary>
        public static Parser<T> Label<T>(Parser<T> parse, string expectation)
        {
            var errors = ErrorMessageList.Empty.With(ErrorMessage.Expected(expectation));

            return tokens =>
            {
                var start = tokens.Position;
                var reply = parse(tokens);
                var newPosition = reply.UnparsedTokens.Position;
                if (start == newPosition)
                {
                    if (reply.Success)
                        reply = new Parsed<T>(reply.Value, reply.UnparsedTokens, errors);
                    else
                        reply = new Error<T>(reply.UnparsedTokens, errors);
                }
                return reply;
            };
        }

        private static IEnumerable<T> List<T>(T first, IEnumerable<T> rest)
        {
            yield return first;

            foreach (T item in rest)
                yield return item;
        }

        protected static Parser<IEnumerable<T>> Zero<T>()
        {
            return Enumerable.Empty<T>().SucceedWithThisValue();
        }
    }

    public static class AbstractGrammarExtensions
    {
        /// <summary>
        /// goal.TerminatedBy(terminator) parse goal and then terminator.  If goal and terminator both
        /// succeed, the result of the goal parser is returned.
        /// </summary>
        public static Parser<T> TerminatedBy<T, S>(this Parser<T> goal, Parser<S> terminator)
        {
            return from G in goal
                   from _ in terminator
                   select G;
        }
    }
}