using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class OperatorPrecedenceParserTests : Grammar
    {
        private static Lexer Tokenize(string source)
        {
            return new SampleLexer(source);
        }

        private OperatorPrecedenceParser<string> expression;
        
        private string List(IEnumerable<string> items)
        {
            return string.Join(", ", items.Select(x => "(" + x + ")"));
        }

        [SetUp]
        public void SetUp()
        {
            expression = new OperatorPrecedenceParser<string>();

            expression.Atom(SampleLexer.Digit, token => token.Literal);
            expression.Atom(SampleLexer.Letter, token => token.Literal);

            expression.Unit(SampleLexer.LeftBrace,
                            from items in Between(Token("["), OneOrMore(expression, Token(",")), Token("]"))
                            select "[" + List(items) + "]");

            expression.Prefix(SampleLexer.Not, 10, (symbol, operand) => "(" + symbol.Literal + "(" + operand + "))");
            expression.Prefix(SampleLexer.Subtract, 10, (symbol, operand) => "(" + symbol.Literal + "(" + operand + "))");

            expression.Extend(SampleLexer.LeftParen, 12, callable =>
                                                     from arguments in Between(Token("("), ZeroOrMore(expression, Token(",")), Token(")"))
                                                     select "(" + callable + ")" + "(" + List(arguments) + ")");

            expression.Binary(SampleLexer.Multiply, 9, (left, symbol, right) => "((" + left + ") * (" + right + "))");
            expression.Binary(SampleLexer.Subtract, 8, (left, symbol, right) => "((" + left + ") - (" + right + "))");
        }

        [Test]
        public void ParsesRegisteredTokensIntoCorrespondingAtoms()
        {
            expression.Parses(Tokenize("1")).IntoValue("1");
            expression.Parses(Tokenize("a")).IntoValue("a");
        }

        [Test]
        public void ParsesUnitExpressionsStartedByRegisteredTokens()
        {
            expression.Parses(Tokenize("[a]")).IntoValue("[(a)]");
            expression.Parses(Tokenize("[a,1,[b,2]]")).IntoValue("[(a), (1), ([(b), (2)])]");
        }

        [Test]
        public void ParsesPrefixExpressionsStartedByRegisteredToken()
        {
            expression.Parses(Tokenize("-1")).IntoValue("(-(1))");
            expression.Parses(Tokenize("!a")).IntoValue("(!(a))");

            expression.Parses(Tokenize("!-1")).IntoValue("(!((-(1))))");
            expression.Parses(Tokenize("-!a")).IntoValue("(-((!(a))))");
        }

        [Test]
        public void ParsesExpressionsThatExtendTheLeftSideExpressionWhenTheRegisteredTokenIsEncountered()
        {
            expression.Parses(Tokenize("x()")).IntoValue("(x)()");
            expression.Parses(Tokenize("x(1)")).IntoValue("(x)((1))");
            expression.Parses(Tokenize("x(1,a)")).IntoValue("(x)((1), (a))");
        }

        [Test]
        public void ParsesLeftAssociativeBinaryOperationsRespectingPrecedence()
        {
            expression.Parses(Tokenize("1*2")).IntoValue("((1) * (2))");
            expression.Parses(Tokenize("1-2")).IntoValue("((1) - (2))");

            expression.Parses(Tokenize("1*2*3")).IntoValue("((((1) * (2))) * (3))");
            expression.Parses(Tokenize("1-2-3")).IntoValue("((((1) - (2))) - (3))");

            expression.Parses(Tokenize("1*2*3-4")).IntoValue("((((((1) * (2))) * (3))) - (4))");
            expression.Parses(Tokenize("1-2-3*4")).IntoValue("((((1) - (2))) - (((3) * (4))))");
        }

        [Test]
        public void ProvidesErrorAtAppropriatePositionWhenUnitParsersFail()
        {
            //Upon unit-parser failures, stop!
            
            //The "[" unit-parser is invoked but fails.  The next token, "*", has
            //high precedence, but that should not provoke parsing to continue.
            
            expression.FailsToParse(Tokenize("[*"), "*").WithMessage("(1, 2): Parse error.");
        }

        [Test]
        public void ProvidesErrorAtAppropriatePositionWhenExtendParsersFail()
        {
            //Upon extend-parser failures, stop!
            
            //The "2" unit-parser succeeds.  The next token, "-" has
            //high-enough precedence to continue, so the "-" extend-parser
            //is invoked and immediately fails.  The next token, "*", has
            //high precedence, but that should not provoke parsing to continue.

            expression.FailsToParse(Tokenize("2-*"), "*").WithMessage("(1, 3): Parse error.");
        }
    }
}
