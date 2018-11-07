using System;
using Parsley.Parsers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Parsley
{
    public abstract class Grammar
    {
        public static IParser<TItem> Fail<TItem>()
            
            => new FailingParser<TItem>();

        public static IParserG EndOfInput
            
            => new TokenByKindGeneralParser(TokenKind.EndOfInput);

        public static IParser<Token> Token(TokenKind kind)
            
            => new TokenByKindParser(kind);

        public static IParserG TokenG(TokenKind kind)

            => new TokenByKindGeneralParser(kind);
        
        public static IParser<Token> Token(string expectation)
            
            => new TokenByLiteralParser(expectation);

        public static IParser<TResult> Token<TResult>(TokenKind kind, Func<string, TResult> resultContinuation)

            => new MapTokenLiteralParser<TResult>(kind, resultContinuation);

        public static IParser<string> TokenLiteral(TokenKind kind)

            => new ReturnTokenLiteralParser(kind);


        /// <summary>
        /// ZeroOrMore(p) repeatedly applies an parser p until it fails, returing
        /// the list of values returned by successful applications of p.  At the
        /// end of the sequence, p must fail without consuming input, otherwise the
        /// sequence will fail with the error reported by p.
        /// </summary>
        public static IParser<IList<TItem>> ZeroOrMore<TItem>(IParser<TItem> item)

            => new QuantifiedParser<TItem>(item, QuantificationRule.NOrMore, 0);

        /// <summary>
        /// OneOrMore(p) behaves like ZeroOrMore(p), except that p must succeed at least one time.
        /// </summary>
        public static IParser<IList<TItem>> OneOrMore<TItem>(IParser<TItem> item)

            => new QuantifiedParser<TItem>(item, QuantificationRule.NOrMore, 1);

        /// <summary>
        /// ZeroOrMore(p, s) parses zero or more occurrences of p separated by occurrences of s,
        /// returning the list of values returned by successful applications of p.
        /// </summary>
        public static IParser<IList<TItem>> ZeroOrMore<TItem>(IParser<TItem> item, IParserG separator)

            => new QuantifiedParser<TItem>(item, QuantificationRule.NOrMore, 0, -1, separator);

        /// <summary>
        /// OneOrMore(p, s) behaves like ZeroOrMore(p, s), except that p must succeed at least one time.
        /// </summary>
        public static IParser<IList<TItem>> OneOrMore<TItem>(IParser<TItem> item, IParserG separator)
            
            => new QuantifiedParser<TItem>(item, QuantificationRule.NOrMore, 1, -1, separator);

        public static IParser<IList<TItem>> NOrMore<TItem>(int n, IParser<TItem> item, IParserG separator)

            => new QuantifiedParser<TItem>(item, QuantificationRule.NOrMore, n, -1, separator);

        public static IParser<IList<TItem>> NOrLess<TItem>(int n, IParser<TItem> item, IParserG separator)

            => new QuantifiedParser<TItem>(item, QuantificationRule.NOrLess, n, -1, separator);

        public static IParser<IList<TItem>> NToMTimes<TItem>(int n, int m, IParser<TItem> item, IParserG separator)
            
            => new QuantifiedParser<TItem>(item, QuantificationRule.NtoM, n, m, separator);

        public static IParser<IList<TItem>> NTimesExactly<TItem>(int n, IParser<TItem> item, IParserG separator)

            => new QuantifiedParser<TItem>(item, QuantificationRule.ExactlyN, n, -1, separator);

        /// <summary>
        /// Between(left, goal, right) parses its arguments in order.  If all three
        /// parsers succeed, the result of the goal parser is returned.
        /// </summary>
        public static IParser<TItem> Between<TItem>(IParserG left, IParser<TItem> item, IParserG right)

            => new BetweenParser<TItem>(left, item, right);

        /// <summary>
        /// Optional(p) is equivalent to p whenever p succeeds or when p fails after consuming input.
        /// If p fails without consuming input, Optional(p) succeeds.
        /// </summary>
        public static IParser<TItem> Optional<TItem>(IParser<TItem> parser, TItem defaultValue = default(TItem))
        
            => new OptionalParser<TItem>(parser, defaultValue);
        
        /// <summary>
        /// The parser Attempt(p) behaves like parser p, except that it pretends
        /// that it hasn't consumed any input when an error occurs. This combinator
        /// is used whenever arbitrary look ahead is needed.
        /// </summary>
        public static IParser<TItem> Attempt<TItem>(IParser<TItem> parse)
            
            => new AttemptParser<TItem>(parse);

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
        public static IParser<TItem> Choice<TItem>(params IParser<TItem>[] parsers)
            
            => new ChoiceParser<TItem>(parsers);

        /// <summary>
        /// When parser p consumes any input, Label(p, e) is the same as p.
        /// When parser p does not consume any input, Label(p, e) is the same
        /// as p, except any messages are replaced with expectation e.
        /// </summary>
        public static IParser<TItem> Label<TItem>(IParser<TItem> parser, string expectation)
            
            => new LabeledParser<TItem>(parser, expectation);

        public static IParser<TItem> Constant<TItem>(TokenKind kind, TItem constant)
            
            => new ConstantParser<TItem>(kind, constant);

        public static IParser<KeyValuePair<TName, TValue>> NameValuePair<TName, TValue>(IParser<TName> name,
            IParserG delimiter, IParser<TValue> value)

            => new NameValuePairParser<TName, TValue>(name, delimiter, value);

        public static IParser<TResult> OccupiesEntireInput<TResult>(IParser<TResult> parser)
        
            => new TakeSkipParser<TResult>(parser, EndOfInput);

        public static IParserG Skip(IParserG parser)

            => parser;
        

        protected void InferGrammarRuleNames()
        {
            var rules =
                GetType()
                    .GetRuntimeFields()
                    .Where(grammarRuleField => grammarRuleField.FieldType.GetInterface(nameof(INamedInternal)) != null)
                    .Select(grammarRuleField => new { Rule = (INamedInternal)grammarRuleField.GetValue(this), grammarRuleField.Name })
                    .Where(ruleName => ruleName.Rule != null);

            foreach (var rule in rules)
            {
                if (rule.Rule.Name == null)
                    rule.Rule.SetName(rule.Name);
            }
        }
    }
}