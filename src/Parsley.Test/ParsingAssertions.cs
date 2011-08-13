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

            lexer.CurrentToken.Kind.ShouldEqual(Lexer.EndOfInput);
        }

        public static void ShouldYieldTokens(this Lexer lexer, params string[] expectedLiterals)
        {
            foreach (var expectedLiteral in expectedLiterals)
            {
                lexer.CurrentToken.Literal.ShouldEqual(expectedLiteral);
                lexer.CurrentToken.Kind.ShouldNotEqual(Lexer.Unknown);
                lexer = lexer.Advance();
            }

            lexer.CurrentToken.Kind.ShouldEqual(Lexer.EndOfInput);
        }

        public static void ShouldBe(this Token actual, TokenKind expectedKind, string expectedLiteral, int expectedLine, int expectedColumn)
        {
            actual.ShouldBe(expectedKind, expectedLiteral);
            actual.Position.Line.ShouldEqual(expectedLine);
            actual.Position.Column.ShouldEqual(expectedColumn);
        }

        public static void ShouldBe(this Token actual, TokenKind expectedKind, string expectedLiteral)
        {
            actual.Kind.ShouldEqual(expectedKind);
            actual.Literal.ShouldEqual(expectedLiteral);
        }

        public static Reply<T> FailsToParse<T>(this Parser<T> parser, Lexer tokens, string expectedUnparsedSource)
        {
            return parser.Parse(tokens).Fails().WithUnparsedText(expectedUnparsedSource);
        }

        private static Reply<T> Fails<T>(this Reply<T> reply)
        {
            reply.Success.ShouldBeFalse("Parse completed without expected error.");

            return reply;
        }

        public static Reply<T> WithMessage<T>(this Reply<T> reply, string expectedMessage)
        {
            var position = reply.UnparsedTokens.Position;
            var actual = String.Format("({0}, {1}): {2}", position.Line, position.Column, reply.ErrorMessages);
            actual.ShouldEqual(expectedMessage);
            return reply;
        }

        public static Reply<T> WithNoMessage<T>(this Reply<T> reply)
        {
            reply.ErrorMessages.ShouldEqual(ErrorMessageList.Empty);
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
            reply.Success.ShouldBeTrue(reply.ErrorMessages.ToString());

            return reply;
        }

        private static Reply<T> WithUnparsedText<T>(this Reply<T> reply, string expected)
        {
            reply.UnparsedTokens.ToString().ShouldEqual(expected);

            return reply;
        }

        private static Reply<T> WithAllInputConsumed<T>(this Reply<T> reply)
        {
            var consumedAllInput = reply.UnparsedTokens.CurrentToken.Kind == Lexer.EndOfInput;
            consumedAllInput.ShouldBeTrue("Did not consume all input.");
            reply.UnparsedTokens.ToString().ShouldEqual("");

            return reply;
        }

        public static Reply<T> IntoValue<T>(this Reply<T> reply, T expected)
        {
            reply.Value.ShouldEqual(expected);

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
            reply.Value.Literal.ShouldEqual(expectedLiteral);

            return reply;
        }

        public static Reply<IEnumerable<Token>> IntoTokens(this Reply<IEnumerable<Token>> reply, params string[] expectedLiterals)
        {
            return reply.IntoValue(tokens => tokens.Select(x => x.Literal).ShouldList(expectedLiterals));
        }
    }
}