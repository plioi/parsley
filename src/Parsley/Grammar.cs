namespace Parsley
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Primitives;

    public abstract class Grammar
    {
        protected void InferGrammarRuleNames()
        {
            var grammarRules =
                GetType()
                    .GetRuntimeFields()
                    .Where(rule =>
                           rule.FieldType.GetTypeInfo().IsGenericType &&
                           rule.FieldType.GetGenericTypeDefinition() == typeof (GrammarRule<>));

            foreach (var rule in grammarRules)
            {
                var value = rule.GetValue(this);
                if (value != null)
                {
                    var nameProperty = value.GetType().GetRuntimeProperty("Name");
                    var name = nameProperty.GetValue(value, null);

                    if (name as string == null)
                        nameProperty.SetValue(value, rule.Name, null);
                }
            }
        }

        public static Parser<T> Fail<T>()
        {
            return new FailingParser<T>();
        }

        public static Parser<Token> EndOfInput
        {
            get { return Token(TokenKind.EndOfInput); }
        }

        public static Parser<Token> Token(TokenKind kind)
        {
            return new TokenByKindParser(kind);
        }

        public static Parser<Token> Token(string expectation)
        {
            return new TokenByLiteralParser(expectation);
        }

        /// <summary>
        /// ZeroOrMore(p) repeatedly applies an parser p until it fails, returing
        /// the list of values returned by successful applications of p.  At the
        /// end of the sequence, p must fail without consuming input, otherwise the
        /// sequence will fail with the error reported by p.
        /// </summary>
        public static Parser<IEnumerable<T>> ZeroOrMore<T>(Parser<T> item)
        {
            return new ZeroOrMoreParser<T>(item);
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
        public static Parser<T> Optional<T>(Parser<T> parser)
        {
            var nothing = default(T).SucceedWithThisValue();
            return Choice(parser, nothing);
        }

        /// <summary>
        /// The parser Attempt(p) behaves like parser p, except that it pretends
        /// that it hasn't consumed any input when an error occurs. This combinator
        /// is used whenever arbitrary look ahead is needed.
        /// </summary>
        public static Parser<T> Attempt<T>(Parser<T> parse)
        {
            return new AttemptParser<T>(parse);
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

            return new ChoiceParser<T>(parsers);
        }

        /// <summary>
        /// When parser p consumes any input, Label(p, e) is the same as p.
        /// When parser p does not consume any input, Label(p, e) is the same
        /// as p, except any messages are replaced with expectation e.
        /// </summary>
        public static Parser<T> Label<T>(Parser<T> parser, string expectation)
        {
            return new LabeledParser<T>(parser, expectation);
        }

        private static IEnumerable<T> List<T>(T first, IEnumerable<T> rest)
        {
            yield return first;

            foreach (T item in rest)
                yield return item;
        }

        private static Parser<IEnumerable<T>> Zero<T>()
        {
            return Enumerable.Empty<T>().SucceedWithThisValue();
        }
    }
}