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
            var ruleNames =
                GetType()
                    .GetRuntimeFields()
                    .Where(grammarRuleField => grammarRuleField.FieldType.BaseType == typeof(GrammarRule))
                    .Select(grammarRuleField => new { Rule = (GrammarRule)grammarRuleField.GetValue(this), grammarRuleField.Name })
                    .Where(ruleName => ruleName.Rule != null);

            foreach (var ruleName in ruleNames)
            {
                if (ruleName.Rule.Name == null)
                    ruleName.Rule.Name = ruleName.Name;
            }
        }

        public static IParser<T> Fail<T>()
        {
            return new FailingParser<T>();
        }

        public static IParser<Token> EndOfInput => Token(TokenKind.EndOfInput);

        public static IParser<Token> Token(TokenKind kind)
        {
            return new TokenByKindParser(kind);
        }

        public static IParser<Token> Token(string expectation)
        {
            return new TokenByLiteralParser(expectation);
        }

        /// <summary>
        /// ZeroOrMore(p) repeatedly applies an parser p until it fails, returing
        /// the list of values returned by successful applications of p.  At the
        /// end of the sequence, p must fail without consuming input, otherwise the
        /// sequence will fail with the error reported by p.
        /// </summary>
        public static IParser<IEnumerable<T>> ZeroOrMore<T>(IParser<T> item)
        {
            return new ZeroOrMoreParser<T>(item);
        }

        /// <summary>
        /// OneOrMore(p) behaves like ZeroOrMore(p), except that p must succeed at least one time.
        /// </summary>
        public static IParser<IEnumerable<T>> OneOrMore<T>(IParser<T> item)
        {
            return from first in item
                   from rest in ZeroOrMore(item)
                   select List(first, rest);
        }

        /// <summary>
        /// ZeroOrMore(p, s) parses zero or more occurrences of p separated by occurrences of s,
        /// returning the list of values returned by successful applications of p.
        /// </summary>
        public static IParser<IEnumerable<T>> ZeroOrMore<T, S>(IParser<T> item, IParser<S> separator)
        {
            return Choice(OneOrMore(item, separator), Zero<T>());
        }

        /// <summary>
        /// OneOrMore(p, s) behaves like ZeroOrMore(p, s), except that p must succeed at least one time.
        /// </summary>
        public static IParser<IEnumerable<T>> OneOrMore<T, S>(IParser<T> item, IParser<S> separator)
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
        public static IParser<TGoal> Between<TLeft, TGoal, TRight>(IParser<TLeft> left, 
                                                                  IParser<TGoal> goal, 
                                                                  IParser<TRight> right)
        {
            return from l in left
                   from g in goal
                   from r in right
                   select g;
        }

        /// <summary>
        /// Optional(p) is equivalent to p whenever p succeeds or when p fails after consuming input.
        /// If p fails without consuming input, Optional(p) succeeds.
        /// </summary>
        public static IParser<T> Optional<T>(IParser<T> parser)
        {
            var nothing = default(T).SucceedWithThisValue();
            return Choice(parser, nothing);
        }

        /// <summary>
        /// The parser Attempt(p) behaves like parser p, except that it pretends
        /// that it hasn't consumed any input when an error occurs. This combinator
        /// is used whenever arbitrary look ahead is needed.
        /// </summary>
        public static IParser<T> Attempt<T>(IParser<T> parse)
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
        public static IParser<T> Choice<T>(params IParser<T>[] parsers)
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
        public static IParser<T> Label<T>(IParser<T> parser, string expectation)
        {
            return new LabeledParser<T>(parser, expectation);
        }

        public static IParser<T> Constant<T>(TokenKind kind, T constant)
        {
            return from _ in Token(kind)
                select constant;
        }

        private static IEnumerable<T> List<T>(T first, IEnumerable<T> rest)
        {
            yield return first;

            foreach (T item in rest)
                yield return item;
        }

        private static IParser<IEnumerable<T>> Zero<T>()
        {
            return Enumerable.Empty<T>().SucceedWithThisValue();
        }
    }
}