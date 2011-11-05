using System;
using System.Collections.Generic;
using System.Linq;

namespace Parsley
{
    public static class ParsingAssertions
    {
        public static void ShouldYieldTokens(this Lexer lexer, TokenKind expectedKind, params string[] expectedLiterals)
        {
            foreach (var expectedLiteral in expectedLiterals)
            {
                lexer.CurrentToken.ShouldBe(expectedKind, expectedLiteral);
                lexer = lexer.Advance();
            }

            AssertEqual(Lexer.EndOfInput, lexer.CurrentToken.Kind);
        }

        public static void ShouldYieldTokens(this Lexer lexer, params string[] expectedLiterals)
        {
            foreach (var expectedLiteral in expectedLiterals)
            {
                AssertTokenLiteralsEqual(expectedLiteral, lexer.CurrentToken.Literal);
                AssertNotEqual(Lexer.Unknown, lexer.CurrentToken.Kind);
                lexer = lexer.Advance();
            }

            AssertEqual(Lexer.EndOfInput, lexer.CurrentToken.Kind);
        }

        public static void ShouldBe(this Token actual, TokenKind expectedKind, string expectedLiteral, int expectedLine, int expectedColumn)
        {
            actual.ShouldBe(expectedKind, expectedLiteral);

            var expectedPosition = new Position(expectedLine, expectedColumn);
            if (actual.Position != expectedPosition)
                throw new AssertionException("token at position " + expectedPosition,
                                             "token at position " + actual.Position);
        }

        public static void ShouldBe(this Token actual, TokenKind expectedKind, string expectedLiteral)
        {
            AssertEqual(expectedKind, actual.Kind);
            AssertTokenLiteralsEqual(expectedLiteral, actual.Literal);
        }

        public static Reply<T> FailsToParse<T>(this Parser<T> parser, Lexer tokens, string expectedUnparsedSource)
        {
            return parser.Parse(tokens).Fails().WithUnparsedText(expectedUnparsedSource);
        }

        private static Reply<T> Fails<T>(this Reply<T> reply)
        {
            if (reply.Success)
                throw new AssertionException("parser failure", "parser completed successfully");

            return reply;
        }

        public static Reply<T> WithMessage<T>(this Reply<T> reply, string expectedMessage)
        {
            var position = reply.UnparsedTokens.Position;
            var actual = position + ": " + reply.ErrorMessages;
            
            if (actual != expectedMessage)
                throw new AssertionException(string.Format("message at {0}", expectedMessage),
                                             string.Format("message at {0}", actual));

            return reply;
        }

        public static Reply<T> WithNoMessage<T>(this Reply<T> reply)
        {
            if (reply.ErrorMessages != ErrorMessageList.Empty)
                throw new AssertionException("no error message", reply.ErrorMessages);

            return reply;
        }

        public static Reply<T> PartiallyParses<T>(this Parser<T> parser, Lexer tokens, string expectedUnparsedSource)
        {
            return parser.Parse(tokens).Succeeds().WithUnparsedText(expectedUnparsedSource);
        }

        public static Reply<T> Parses<T>(this Parser<T> parser, Lexer tokens)
        {
            return parser.Parse(tokens).Succeeds().WithAllInputConsumed();
        }

        private static Reply<T> Succeeds<T>(this Reply<T> reply)
        {
            if (!reply.Success)
                throw new AssertionException(reply.ErrorMessages.ToString(), "parser success", "parser failed");

            return reply;
        }

        private static Reply<T> WithUnparsedText<T>(this Reply<T> reply, string expected)
        {
            var actual = reply.UnparsedTokens.ToString();
            
            if (actual != expected)
                throw new AssertionException(string.Format("remaining unparsed text \"{0}\"", expected),
                                             string.Format("remaining unparsed text \"{0}\"", actual));

            return reply;
        }

        private static Reply<T> WithAllInputConsumed<T>(this Reply<T> reply)
        {
            var nextTokenKind = reply.UnparsedTokens.CurrentToken.Kind;
            AssertEqual(Lexer.EndOfInput, nextTokenKind);
            return reply.WithUnparsedText("");
        }

        public static Reply<T> IntoValue<T>(this Reply<T> reply, T expected)
        {
            if (!Equals(expected, reply.Value))
                throw new AssertionException(string.Format("parsed value: {0}", expected),
                                             string.Format("parsed value: {0}", reply.Value));

            return reply;
        }

        public static Reply<T> IntoValue<T>(this Reply<T> reply, Action<T> assertParsedValue)
        {
            assertParsedValue(reply.Value);

            return reply;
        }

        public static Reply<Token> IntoToken(this Reply<Token> reply, TokenKind expectedKind, string expectedLiteral)
        {
            reply.Value.ShouldBe(expectedKind, expectedLiteral);

            return reply;
        }

        public static Reply<Token> IntoToken(this Reply<Token> reply, string expectedLiteral)
        {
            AssertTokenLiteralsEqual(expectedLiteral, reply.Value.Literal);
            return reply;
        }

        public static Reply<IEnumerable<Token>> IntoTokens(this Reply<IEnumerable<Token>> reply, params string[] expectedLiterals)
        {
            var actualLiterals = reply.Value.Select(x => x.Literal).ToArray();

            Action raiseError = () =>
            {
                throw new AssertionException("Parse resulted in unexpected token literals.",
                                             String.Join(", ", expectedLiterals),
                                             String.Join(", ", actualLiterals));
            };

            if (actualLiterals.Length != expectedLiterals.Length)
                raiseError();

            for (int i = 0; i < actualLiterals.Length; i++)
                if (actualLiterals[i] != expectedLiterals[i])
                    raiseError();

            return reply;
        }

        private static void AssertTokenLiteralsEqual(string expected, string actual)
        {
            if (actual != expected)
                throw new AssertionException(string.Format("token with literal \"{0}\"", expected),
                                             string.Format("token with literal \"{0}\"", actual));
        }

        private static void AssertEqual(TokenKind expected, TokenKind actual)
        {
            if (actual != expected)
                throw new AssertionException(string.Format("<{0}> token", expected),
                                             string.Format("<{0}> token", actual));
        }

        private static void AssertNotEqual(TokenKind expected, TokenKind actual)
        {
            if (actual == expected)
                throw new AssertionException(string.Format("not <{0}> token", expected),
                                             string.Format("<{0}> token", actual));
        }
    }
}