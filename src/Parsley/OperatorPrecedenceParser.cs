using Parsley.Parsers;
using System.Collections.Generic;
using static Parsley.Grammar;

namespace Parsley
{
    public delegate IParser<T> ExtendParserBuilder<T>(T left);
    public delegate T AtomNodeBuilder<out T>(string atom);
    public delegate T UnaryNodeBuilder<T>(string symbol, T operand);
    public delegate T BinaryNodeBuilder<T>(T left, string symbol, T right);
    public enum Associativity { Left, Right }

    public class OperatorPrecedenceParser<T> : Parser<T>
    {
        private readonly IDictionary<TokenKind, IParser<T>> _unitParsers;
        private readonly IDictionary<TokenKind, ExtendParserBuilder<T>> _extendParsers;
        private readonly IDictionary<TokenKind, int> _extendParserPrecedence;

        public OperatorPrecedenceParser()
        {
            _unitParsers = new Dictionary<TokenKind, IParser<T>>();
            _extendParsers = new Dictionary<TokenKind, ExtendParserBuilder<T>>();
            _extendParserPrecedence = new Dictionary<TokenKind, int>();
        }

        public void Unit(TokenKind kind, IParser<T> unitParser)
        {
            _unitParsers[kind] = unitParser;
        }

        public void Atom(TokenKind kind, AtomNodeBuilder<T> createAtomNode)
        {
            Unit(kind, kind.Literal(l => createAtomNode(l)));
        }

        public void Prefix(TokenKind operation, int precedence, UnaryNodeBuilder<T> createUnaryNode)
        {
            Unit(operation, from symbol in operation.Literal()
                            from operand in OperandAtPrecedenceLevel(precedence)
                            select createUnaryNode(symbol, operand));
        }

        public void Extend(TokenKind operation, int precedence, ExtendParserBuilder<T> createExtendParser)
        {
            _extendParsers[operation] = createExtendParser;
            _extendParserPrecedence[operation] = precedence;
        }

        public void Postfix(TokenKind operation, int precedence, UnaryNodeBuilder<T> createUnaryNode)
        {
            Extend(operation, precedence, left => from symbol in operation.Literal()
                                                  select createUnaryNode(symbol, left));
        }

        public void Binary(TokenKind operation, int precedence, BinaryNodeBuilder<T> createBinaryNode,
                           Associativity associativity = Associativity.Left)
        {
            int rightOperandPrecedence = precedence;

            if (associativity == Associativity.Right)
                rightOperandPrecedence = precedence - 1;

            Extend(operation, precedence, left => from symbol in operation.Literal()
                                                  from right in OperandAtPrecedenceLevel(rightOperandPrecedence)
                                                  select createBinaryNode(left, symbol, right));
        }

        public override IReply<T> Parse(TokenStream tokens)
        {
            return Parse(tokens, 0);
        }

        private IParser<T> OperandAtPrecedenceLevel(int precedence)
        {
            return new LambdaParser<T>(tokens => Parse(tokens, precedence));
        }

        private IReply<T> Parse(TokenStream tokens, int precedence)
        {
            var token = tokens.Current;

            if (!_unitParsers.ContainsKey(token.Kind))
                return new Error<T>(tokens, ErrorMessage.Unknown());

            var reply = _unitParsers[token.Kind].Parse(tokens);

            if (!reply.Success)
                return reply;

            tokens = reply.UnparsedTokens;
            token = tokens.Current;

            while (precedence < GetPrecedence(token))
            {
                //Continue parsing at this precedence level.

                reply = _extendParsers[token.Kind](reply.Value).Parse(tokens);

                if (!reply.Success)
                    return reply;

                tokens = reply.UnparsedTokens;
                token = tokens.Current;
            }

            return reply;
        }

        private int GetPrecedence(Token token)
        {
            var kind = token.Kind;
            if (_extendParserPrecedence.ContainsKey(kind))
                return _extendParserPrecedence[kind];

            return 0;
        }

        protected override string GetName() => "<OPP>";
    }
}
