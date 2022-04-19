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

    public Parser<T> Parser
        => (ref Text input, ref Position position, [NotNullWhen(true)] out T? value, [NotNullWhen(false)] out string? expectation)
                => Parse(ref input, ref position, 0, out value, out expectation);

    Parser<T> OperandAtPrecedenceLevel(int precedence)
        => (ref Text input, ref Position position, [NotNullWhen(true)] out T? value, [NotNullWhen(false)] out string? expectation)
            => Parse(ref input, ref position, precedence, out value, out expectation);

    bool Parse(ref Text input, ref Position position, int precedence, [NotNullWhen(true)] out T? value, [NotNullWhen(false)] out string? expectation)
    {
        if (!TryFindMatchingUnitParser(ref input, ref position, out var matchingUnitParser, out var token))
        {
            expectation = "expression";
            value = default;
            return false;
        }

        if (matchingUnitParser(ref input, ref position, out value, out expectation))
        {
            while (TryFindMatchingExtendParserBuilder(ref input, ref position, out var matchingExtendParserBuilder, out token, out int? tokenPrecedence) && precedence < tokenPrecedence)
            {
                //Continue parsing at this precedence level.

                var extendParser = matchingExtendParserBuilder(value);

                if (extendParser(ref input, ref position, out value, out expectation))
                    continue;

                return false;
            }

            return true;
        }

        return false;
    }

    bool TryFindMatchingUnitParser(ref Text input, ref Position position, [NotNullWhen(true)] out Parser<T>? found, out string? token)
    {
        found = null;
        token = null;

        foreach(var (kind, parser) in unitParsers)
        {
            var snapshot = input;
            var originalPosition = position;
            bool searchSucceeded = kind(ref input, ref position, out var value, out _);
            input = snapshot;
            position = originalPosition;

            if (searchSucceeded)
            {
                token = value;
                found = parser;
                return true;
            }
        }

        return false;
    }

    bool TryFindMatchingExtendParserBuilder(ref Text input, ref Position position, [NotNullWhen(true)] out ExtendParserBuilder<T>? found, out string? token, out int? tokenPrecedence)
    {
        found = null;
        token = null;
        tokenPrecedence = null;

        foreach (var (kind, precedence, extendParserBuilder) in extendParsers)
        {
            var snapshot = input;
            var originalPosition = position;
            bool searchSucceeded = kind(ref input, ref position, out token, out _);
            input = snapshot;
            position = originalPosition;

            if (searchSucceeded)
            {
                tokenPrecedence = precedence;
                found = extendParserBuilder;
                return true;
            }
        }

        return false;
    }
}
