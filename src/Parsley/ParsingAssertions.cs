﻿namespace Parsley
{
    using System;
    using System.Collections.Generic;

    public static class ParsingAssertions
    {
        public static void ShouldSucceed(this MatchResult actual, string expected)
        {
            if (!actual.Success)
                throw new AssertionException("successful match", "match failed");

            if (actual.Value != expected)
                throw new AssertionException(expected, actual.Value);
        }

        public static void ShouldFail(this MatchResult actual)
        {
            if (actual.Success)
                throw new AssertionException("match failure", "successful match");

            const string expected = "";

            if (actual.Value != expected)
                throw new AssertionException(expected, actual.Value);
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

        public static IReply<T> FailsToParse<T>(this IParser<T> parser, IEnumerable<Token> tokens)
        {
            var reply = parser.Parse(new TokenStream(tokens));
            
            if (reply.Success)
                throw new AssertionException("parser failure", "parser completed successfully");

            return reply;
        }

        public static IReply<T> WithMessage<T>(this IReply<T> reply, string expectedMessage)
        {
            var position = reply.UnparsedTokens.Position;
            var actual = position + ": " + reply.ErrorMessages;
            
            if (actual != expectedMessage)
                throw new AssertionException($"message at {expectedMessage}", $"message at {actual}");

            return reply;
        }

        public static IReply<T> WithNoMessage<T>(this IReply<T> reply)
        {
            if (reply.ErrorMessages != ErrorMessageList.Empty)
                throw new AssertionException("no error message", reply.ErrorMessages);

            return reply;
        }

        public static IReply<T> PartiallyParses<T>(this IParser<T> parser, IEnumerable<Token> tokens)
        {
            return parser.Parse(new TokenStream(tokens)).Succeeds();
        }

        public static IReply<T> Parses<T>(this IParser<T> parser, IEnumerable<Token> tokens)
        {
            return parser.Parse(new TokenStream(tokens)).Succeeds().AtEndOfInput();
        }

        private static IReply<T> Succeeds<T>(this IReply<T> reply)
        {
            if (!reply.Success)
            {
                var message = "Position: " + reply.UnparsedTokens.Position
                              + Environment.NewLine
                              + "Error Message: " + reply.ErrorMessages;
                throw new AssertionException(message, "parser success", "parser failed");
            }

            return reply;
        }

        public static IReply<T> LeavingUnparsedTokens<T>(this IReply<T> reply, params string[] expectedLiterals)
        {
            var stream = reply.UnparsedTokens;

            var actualLiterals = new List<string>();

            while (stream.Current.Kind != TokenKind.EndOfInput)
            {
                actualLiterals.Add(stream.Current.Literal);
                stream = stream.Advance();
            }

            void RaiseError()
            {
                throw new AssertionException("Parse resulted in unexpected remaining unparsed tokens.", string.Join(", ", expectedLiterals), string.Join(", ", actualLiterals));
            }

            if (actualLiterals.Count != expectedLiterals.Length)
                RaiseError();

            for (int i = 0; i < actualLiterals.Count; i++)
                if (actualLiterals[i] != expectedLiterals[i])
                    RaiseError();

            return reply;
        }

        public static IReply<T> AtEndOfInput<T>(this IReply<T> reply)
        {
            var nextTokenKind = reply.UnparsedTokens.Current.Kind;
            AssertEqual(TokenKind.EndOfInput, nextTokenKind);
            return reply.LeavingUnparsedTokens(new string[] {});
        }

        public static IReply<T> WithValue<T>(this IReply<T> reply, T expected)
        {
            if (!Equals(expected, reply.Value))
                throw new AssertionException($"parsed value: {expected}", $"parsed value: {reply.Value}");

            return reply;
        }

        public static IReply<T> WithValue<T>(this IReply<T> reply, Action<T> assertParsedValue)
        {
            assertParsedValue(reply.Value);

            return reply;
        }

        private static void AssertTokenLiteralsEqual(string expected, string actual)
        {
            if (actual != expected)
                throw new AssertionException($"token with literal \"{expected}\"", $"token with literal \"{actual}\"");
        }

        private static void AssertEqual(TokenKind expected, TokenKind actual)
        {
            if (actual != expected)
                throw new AssertionException($"<{expected}> token", $"<{actual}> token");
        }
    }
}