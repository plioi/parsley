namespace Parsley
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

        public static Reply<T> FailsToParse<T>(this Parser<T> parser, IEnumerable<Token> tokens)
        {
            var reply = parser.Parse(new TokenStream(tokens));
            
            if (reply.Success)
                throw new AssertionException("parser failure", "parser completed successfully");

            return reply;
        }

        public static Reply<T> WithMessage<T>(this Reply<T> reply, string expectedMessage)
        {
            var position = reply.UnparsedTokens.Position;
            var actual = position + ": " + reply.ErrorMessages;
            
            if (actual != expectedMessage)
                throw new AssertionException($"message at {expectedMessage}", $"message at {actual}");

            return reply;
        }

        public static Reply<T> WithNoMessage<T>(this Reply<T> reply)
        {
            if (reply.ErrorMessages != ErrorMessageList.Empty)
                throw new AssertionException("no error message", reply.ErrorMessages);

            return reply;
        }

        public static Reply<T> PartiallyParses<T>(this Parser<T> parser, IEnumerable<Token> tokens)
        {
            return parser.Parse(new TokenStream(tokens)).Succeeds();
        }

        public static Reply<T> Parses<T>(this Parser<T> parser, IEnumerable<Token> tokens)
        {
            return parser.Parse(new TokenStream(tokens)).Succeeds().AtEndOfInput();
        }

        private static Reply<T> Succeeds<T>(this Reply<T> reply)
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

        public static Reply<T> LeavingUnparsedTokens<T>(this Reply<T> reply, params string[] expectedLiterals)
        {
            var stream = reply.UnparsedTokens;

            var actualLiterals = new List<string>();

            while (stream.Current.Kind != TokenKind.EndOfInput)
            {
                actualLiterals.Add(stream.Current.Literal);
                stream = stream.Advance();
            }

            Action raiseError = () =>
            {
                throw new AssertionException("Parse resulted in unexpected remaining unparsed tokens.",
                                             String.Join(", ", expectedLiterals),
                                             String.Join(", ", actualLiterals));
            };

            if (actualLiterals.Count != expectedLiterals.Length)
                raiseError();

            for (int i = 0; i < actualLiterals.Count; i++)
                if (actualLiterals[i] != expectedLiterals[i])
                    raiseError();

            return reply;
        }

        public static Reply<T> AtEndOfInput<T>(this Reply<T> reply)
        {
            var nextTokenKind = reply.UnparsedTokens.Current.Kind;
            AssertEqual(TokenKind.EndOfInput, nextTokenKind);
            return reply.LeavingUnparsedTokens(new string[] {});
        }

        public static Reply<T> WithValue<T>(this Reply<T> reply, T expected)
        {
            if (!Equals(expected, reply.Value))
                throw new AssertionException($"parsed value: {expected}", $"parsed value: {reply.Value}");

            return reply;
        }

        public static Reply<T> WithValue<T>(this Reply<T> reply, Action<T> assertParsedValue)
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