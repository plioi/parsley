using Parsley.Primitives;
using Shouldly;
using System.Text;
using Xunit;

namespace Parsley.Tests
{
    public class QuantifiedParserTests
    {
        private static string Asterisc = "*";
        private static readonly TokenKind AsteriscToken = new Operator(Asterisc);
        private static readonly Lexer AsteriscLexer = new Lexer(AsteriscToken);
        private static readonly IParser<Token> AsteriscParser = Grammar.Token(AsteriscToken);

        [Fact]
        public void AtLeastNTimes()
        {
            for (int n = 0; n < 10; ++n)
            {
                var parser = new QuantifiedParser<Token, Token>(AsteriscParser, QuantificationRule.NOrMore, n);

                for (var i = n; i < n + 15; ++i)
                    parser.Parse(AsteriscStream(i)).Success.ShouldBe(true);

                for (var i = n - 1; i >= 0; --i)
                    parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);
            }
        }

        [Fact]
        public void ExactlyNTimes()
        {
            for (int n = 0; n < 15; ++n)
            {
                var parser = new QuantifiedParser<Token, Token>(AsteriscParser, QuantificationRule.ExactlyN, n);

                for (var i = n - 1; i >= 0; --i)
                    parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);

                parser.Parse(AsteriscStream(n)).Success.ShouldBe(true);

                for (var i = n + 1; i < n + 15; ++i)
                    parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);
            }
        }

        [Fact]
        public void FromNtoMTimes()
        {
            for (int n = 0; n < 15; ++n)
                for (int m = n; m < n + 10; ++m)
                {
                    var parser = new QuantifiedParser<Token, Token>(AsteriscParser, QuantificationRule.NtoM, n, m);

                    for (var i = 0; i < n; ++i)
                        parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);

                    for (var i = n; i <= m; ++i)
                        parser.Parse(AsteriscStream(i)).Success.ShouldBe(true);

                    for (var i = m + 1; i < m + 15; ++i)
                        parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);
                }
        }

        [Fact]
        public void NoMoreThanNTimes()
        {
            for (int n = 0; n < 15; ++n)
            {
                var parser = new QuantifiedParser<Token, Token>(AsteriscParser, QuantificationRule.NOrLess, n);

                for (var i = 0; i <= n ; ++i)
                    parser.Parse(AsteriscStream(i)).Success.ShouldBe(true);

                for (var i = n + 1; i <= n + 15; ++i)
                    parser.Parse(AsteriscStream(i)).Success.ShouldBe(false);
            }
        }

        static TokenStream AsteriscStream(int n)
        {
            string GenerateAsteriscs(int nn)
            {
                var sb = new StringBuilder();

                for (var i = 0; i < nn; ++i)
                    sb.Append(Asterisc);

                return sb.ToString();
            }

            return new TokenStream(AsteriscLexer.Tokenize(GenerateAsteriscs(n)));
        }
    }
}
