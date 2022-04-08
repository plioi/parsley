using static Parsley.Grammar;

namespace Parsley;

public delegate IParser<T> ExtendParserBuilder<T>(T left);
public delegate T AtomNodeBuilder<out T>(Token atom);
public delegate T UnaryNodeBuilder<T>(Token symbol, T operand);
public delegate T BinaryNodeBuilder<T>(T left, Token symbol, T right);
public enum Associativity { Left, Right }

public class OperatorPrecedenceParser<T> : IParser<T>
{
    readonly List<(TokenKind, IParser<T>)> unitParsers = new();
    readonly List<(TokenKind, ExtendParserBuilder<T>)> extendParsers = new();
    readonly Dictionary<TokenKind, int> extendParserPrecedence = new();

    public void Unit(TokenKind kind, IParser<T> unitParser)
    {
        unitParsers.Add((kind, unitParser));
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
        extendParsers.Add((operation, createExtendParser));
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

    public Reply<T> Parse(Input input)
    {
        return Parse(input, 0);
    }

    IParser<T> OperandAtPrecedenceLevel(int precedence)
    {
        return new LambdaParser<T>(input => Parse(input, precedence));
    }

    Reply<T> Parse(Input input, int precedence)
    {
        var token = input.Current;

        IParser<T> matchingUnitParser = null;
        foreach(var (kind, parser) in unitParsers)
        {
            if (kind == token.Kind)
            {
                matchingUnitParser = parser;
                break;
            }
        }

        if (matchingUnitParser == null)
            return new Error<T>(input, ErrorMessage.Unknown());

        var reply = matchingUnitParser.Parse(input);

        if (!reply.Success)
            return reply;

        input = reply.UnparsedInput;
        token = input.Current;

        while (precedence < GetPrecedence(token))
        {
            //Continue parsing at this precedence level.

            ExtendParserBuilder<T> matchingExtendParserBuilder = null;

            foreach (var (kind, extendParserBuilder) in extendParsers)
            {
                if (kind == token.Kind)
                {
                    matchingExtendParserBuilder = extendParserBuilder;
                    break;
                }
            }

            if (matchingExtendParserBuilder == null)
                return new Error<T>(input, ErrorMessage.Unknown());

            var extendParser = matchingExtendParserBuilder(reply.Value);

            reply = extendParser.Parse(input);

            if (!reply.Success)
                return reply;

            input = reply.UnparsedInput;
            token = input.Current;
        }

        return reply;
    }

    int GetPrecedence(Token token)
    {
        var kind = token.Kind;
        if (extendParserPrecedence.ContainsKey(kind))
            return extendParserPrecedence[kind];

        return 0;
    }
}
