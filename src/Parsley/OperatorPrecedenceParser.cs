namespace Parsley
{
    using System.Collections.Generic;

    public delegate Parser<T> ExtendParserBuilder<T>(T left);
    public delegate T AtomNodeBuilder<out T>(Token atom);
    public delegate T UnaryNodeBuilder<T>(Token symbol, T operand);
    public delegate T BinaryNodeBuilder<T>(T left, Token symbol, T right);
    public enum Associativity { Left, Right }

    public class OperatorPrecedenceParser<T> : Grammar, Parser<T>
    {
        private readonly IDictionary<TokenKind, Parser<T>> unitParsers;
        private readonly IDictionary<TokenKind, ExtendParserBuilder<T>> extendParsers;
        private readonly IDictionary<TokenKind, int> extendParserPrecedence;

        public OperatorPrecedenceParser()
        {
            unitParsers = new Dictionary<TokenKind, Parser<T>>();
            extendParsers = new Dictionary<TokenKind, ExtendParserBuilder<T>>();
            extendParserPrecedence = new Dictionary<TokenKind, int>();
        }

        public void Unit(TokenKind kind, Parser<T> unitParser)
        {
            unitParsers[kind] = unitParser;
        }

        public void Atom(TokenKind kind, AtomNodeBuilder<T> createAtomNode)
        {
            Unit(kind, from token in Token(kind)
                       select createAtomNode(token));
        }

        public void Prefix(TokenKind operation, int precedence, UnaryNodeBuilder<T> createUnaryNode)
        {
            Unit(operation, from symbol in Token(operation)
                            from operand in OperandAtPrecedenceLevel(precedence)
                            select createUnaryNode(symbol, operand));
        }

        public void Extend(TokenKind operation, int precedence, ExtendParserBuilder<T> createExtendParser)
        {
            extendParsers[operation] = createExtendParser;
            extendParserPrecedence[operation] = precedence;
        }

        public void Postfix(TokenKind operation, int precedence, UnaryNodeBuilder<T> createUnaryNode)
        {
            Extend(operation, precedence, left => from symbol in Token(operation)
                                                  select createUnaryNode(symbol, left));
        }

        public void Binary(TokenKind operation, int precedence, BinaryNodeBuilder<T> createBinaryNode,
                           Associativity associativity = Associativity.Left)
        {
            int rightOperandPrecedence = precedence;

            if (associativity == Associativity.Right)
                rightOperandPrecedence = precedence - 1;

            Extend(operation, precedence, left => from symbol in Token(operation)
                                                  from right in OperandAtPrecedenceLevel(rightOperandPrecedence)
                                                  select createBinaryNode(left, symbol, right));
        }

        public Reply<T> Parse(TokenStream tokens)
        {
            return Parse(tokens, 0);
        }

        private Parser<T> OperandAtPrecedenceLevel(int precedence)
        {
            return new LambdaParser<T>(tokens => Parse(tokens, precedence));
        }

        private Reply<T> Parse(TokenStream tokens, int precedence)
        {
            var token = tokens.Current;

            if (!unitParsers.ContainsKey(token.Kind))
                return new Error<T>(tokens, ErrorMessage.Unknown());

            var reply = unitParsers[token.Kind].Parse(tokens);

            if (!reply.Success)
                return reply;

            tokens = reply.UnparsedTokens;
            token = tokens.Current;

            while (precedence < GetPrecedence(token))
            {
                //Continue parsing at this precedence level.

                reply = extendParsers[token.Kind](reply.Value).Parse(tokens);

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
            if (extendParserPrecedence.ContainsKey(kind))
                return extendParserPrecedence[kind];

            return 0;
        }
    }
}
