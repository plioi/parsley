using System;
using System.Collections.Generic;

namespace Parsley
{
    public static class ParsingAssertions
    {
        public static void ShouldEqual(this Token actual, TokenKind expectedKind, string expectedLiteral, int expectedLine, int expectedColumn)
        {
            actual.ShouldEqual(expectedKind, expectedLiteral);

            var expectedPosition = new Position(expectedLine, expectedColumn);
            if (actual.Position != expectedPosition)
                throw new AssertionException("token at position " + expectedPosition,
                                             "token at position " + actual.Position);
        }

        public static void ShouldEqual(this Token actual, TokenKind expectedKind, string expectedLiteral)
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
                throw new AssertionException(reply.ErrorMessages.ToString(), "parser success", "parser failed");

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
                throw new AssertionException(string.Format("parsed value: {0}", expected),
                                             string.Format("parsed value: {0}", reply.Value));

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
                throw new AssertionException(string.Format("token with literal \"{0}\"", expected),
                                             string.Format("token with literal \"{0}\"", actual));
        }

        private static void AssertEqual(TokenKind expected, TokenKind actual)
        {
            if (actual != expected)
                throw new AssertionException(string.Format("<{0}> token", expected),
                                             string.Format("<{0}> token", actual));
        }
    }
}