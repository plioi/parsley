using System.Collections.Generic;
using Xunit;

namespace Parsley.Tests
{
    public class NameValuePairParserTests : Grammar
    {
        [Fact]
        public void ParsesNameValuePairs()
        {
            var parser = NameValuePair(Token(NvpLexer.Name), Token(NvpLexer.Delimiter), Token(NvpLexer.Value));

            parser.Parses(Tokenize("A=B")).AtEndOfInput();

            parser.FailsToParse(Tokenize("AA=B")).WithMessage("(1, 2): = expected");
            parser.FailsToParse(Tokenize("A==B")).WithMessage("(1, 3): B expected");
            parser.FailsToParse(Tokenize("=B")).WithMessage("(1, 1): A expected");
            parser.FailsToParse(Tokenize("A=")).WithMessage("(1, 3): B expected");
        }

        private class NvpLexer : Lexer
        {
            public NvpLexer()
                : base(Name, Delimiter, Value)
            {
            }

            public static readonly TokenKind Name = new Operator("A");
            public static readonly TokenKind Delimiter = new Operator("=");
            public static readonly TokenKind Value = new Operator("B");
        }

        IEnumerable<Token> Tokenize(string text)
            => new NvpLexer().Tokenize(text);
    }
}
