using System.Diagnostics.CodeAnalysis;

namespace Parsley;

public delegate Parser<T> ExtendParserBuilder<T>(T left);
public delegate T AtomNodeBuilder<out T>(string atom);
public delegate T UnaryNodeBuilder<T>(string symbol, T operand);
public delegate T BinaryNodeBuilder<T>(T left, string symbol, T right);
public enum Associativity { Left, Right }

public class OperatorPrecedenceParser<T>
{
    readonly List<(Parser<string>, Parser<T>)> unitParsers = new();
    readonly List<(Parser<string>, int precedence, ExtendParserBuilder<T>)> extendParsers = new();

    public void Unit(Parser<string> kind, Parser<T> unitParser)
    {
        unitParsers.Add((kind, unitParser));
    }

    public void Atom(Parser<string> kind, AtomNodeBuilder<T> createAtomNode)
    {
        Unit(kind, from token in kind
            select createAtomNode(token));
    }

    public void Prefix(Parser<string> operation, int precedence, UnaryNodeBuilder<T> createUnaryNode)
    {
        Unit(operation, from symbol in operation
            from operand in OperandAtPrecedenceLevel(precedence)
            select createUnaryNode(symbol, operand));
    }

    public void Extend(Parser<string> operation, int precedence, ExtendParserBuilder<T> createExtendParser)
    {
        extendParsers.Add((operation, precedence, createExtendParser));
    }

    public void Postfix(Parser<string> operation, int precedence, UnaryNodeBuilder<T> createUnaryNode)
    {
        Extend(operation, precedence, left => from symbol in operation
            select createUnaryNode(symbol, left));
    }

    public void Binary(Parser<string> operation, int precedence, BinaryNodeBuilder<T> createBinaryNode,
                       Associativity associativity = Associativity.Left)
    {
        int rightOperandPrecedence = precedence;

        if (associativity == Associativity.Right)
            rightOperandPrecedence = precedence - 1;

        Extend(operation, precedence, left => from symbol in operation
            from right in OperandAtPrecedenceLevel(rightOperandPrecedence)
            select createBinaryNode(left, symbol, right));
    }

    public Parser<T> Parser => (ref Text input) => Parse(ref input, 0);

    Parser<T> OperandAtPrecedenceLevel(int precedence)
    {
        return (ref Text input) => Parse(ref input, precedence);
    }

    Reply<T> Parse(ref Text input, int precedence)
    {
        if (!TryFindMatchingUnitParser(ref input, out var matchingUnitParser, out var token))
            return new Error<T>("expression");

        var reply = matchingUnitParser(ref input);

        if (!reply.Success)
            return reply;

        while (TryFindMatchingExtendParserBuilder(ref input, out var matchingExtendParserBuilder, out token, out int? tokenPrecedence) && precedence < tokenPrecedence)
        {
            //Continue parsing at this precedence level.

            var extendParser = matchingExtendParserBuilder(reply.Value);

            reply = extendParser(ref input);

            if (!reply.Success)
                return reply;
        }

        return reply;
    }

    bool TryFindMatchingUnitParser(ref Text input, [NotNullWhen(true)] out Parser<T>? found, out string? token)
    {
        found = null;
        token = null;

        foreach(var (kind, parser) in unitParsers)
        {
            var snapshot = input;
            var reply = kind(ref input);
            input = snapshot;

            if (reply.Success)
            {
                token = reply.Value;
                found = parser;
                return true;
            }
        }

        return false;
    }

    bool TryFindMatchingExtendParserBuilder(ref Text input, [NotNullWhen(true)] out ExtendParserBuilder<T>? found, out string? token, out int? tokenPrecedence)
    {
        found = null;
        token = null;
        tokenPrecedence = null;

        foreach (var (kind, precedence, extendParserBuilder) in extendParsers)
        {
            var snapshot = input;
            var reply = kind(ref input);
            input = snapshot;

            if (reply.Success)
            {
                token = reply.Value;
                tokenPrecedence = precedence;
                found = extendParserBuilder;
                return true;
            }
        }

        return false;
    }
}
