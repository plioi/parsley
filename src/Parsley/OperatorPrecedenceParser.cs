using System.Diagnostics.CodeAnalysis;

namespace Parsley;

public delegate Parser<TValue> ExtendParserBuilder<TValue>(TValue left);
public delegate TValue AtomNodeBuilder<out TValue>(string atom);
public delegate TValue UnaryNodeBuilder<TValue>(string symbol, TValue operand);
public delegate TValue BinaryNodeBuilder<TValue>(TValue left, string symbol, TValue right);
public enum Associativity { Left, Right }

public class OperatorPrecedenceParser<TValue>
{
    readonly List<(Parser<string>, Parser<TValue>)> unitParsers = new();
    readonly List<(Parser<string>, int precedence, ExtendParserBuilder<TValue>)> extendParsers = new();

    public void Unit(Parser<string> kind, Parser<TValue> unitParser)
    {
        unitParsers.Add((kind, unitParser));
    }

    public void Atom(Parser<string> kind, AtomNodeBuilder<TValue> createAtomNode)
    {
        Unit(kind, from token in kind
            select createAtomNode(token));
    }

    public void Prefix(Parser<string> operation, int precedence, UnaryNodeBuilder<TValue> createUnaryNode)
    {
        Unit(operation, from symbol in operation
            from operand in OperandAtPrecedenceLevel(precedence)
            select createUnaryNode(symbol, operand));
    }

    public void Extend(Parser<string> operation, int precedence, ExtendParserBuilder<TValue> createExtendParser)
    {
        extendParsers.Add((operation, precedence, createExtendParser));
    }

    public void Postfix(Parser<string> operation, int precedence, UnaryNodeBuilder<TValue> createUnaryNode)
    {
        Extend(operation, precedence, left => from symbol in operation
            select createUnaryNode(symbol, left));
    }

    public void Binary(Parser<string> operation, int precedence, BinaryNodeBuilder<TValue> createBinaryNode,
                       Associativity associativity = Associativity.Left)
    {
        int rightOperandPrecedence = precedence;

        if (associativity == Associativity.Right)
            rightOperandPrecedence = precedence - 1;

        Extend(operation, precedence, left => from symbol in operation
            from right in OperandAtPrecedenceLevel(rightOperandPrecedence)
            select createBinaryNode(left, symbol, right));
    }

    public Parser<TValue> Parser
        => (ref ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out TValue? value, [NotNullWhen(false)] out string? expectation)
                => Parse(ref input, ref index, 0, out value, out expectation);

    Parser<TValue> OperandAtPrecedenceLevel(int precedence)
        => (ref ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out TValue? value, [NotNullWhen(false)] out string? expectation)
            => Parse(ref input, ref index, precedence, out value, out expectation);

    bool Parse(ref ReadOnlySpan<char> input, ref int index, int precedence, [NotNullWhen(true)] out TValue? value, [NotNullWhen(false)] out string? expectation)
    {
        if (!TryFindMatchingUnitParser(ref input, ref index, out var matchingUnitParser, out var token))
        {
            expectation = "expression";
            value = default;
            return false;
        }

        if (matchingUnitParser(ref input, ref index, out value, out expectation))
        {
            while (TryFindMatchingExtendParserBuilder(ref input, ref index, out var matchingExtendParserBuilder, out token, out int? tokenPrecedence) && precedence < tokenPrecedence)
            {
                //Continue parsing at this precedence level.

                var extendParser = matchingExtendParserBuilder(value);

                if (extendParser(ref input, ref index, out value, out expectation))
                    continue;

                return false;
            }

            return true;
        }

        return false;
    }

    bool TryFindMatchingUnitParser(ref ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out Parser<TValue>? found, out string? token)
    {
        found = null;
        token = null;

        foreach(var (kind, parser) in unitParsers)
        {
            var originalIndex = index;
            bool searchSucceeded = kind(ref input, ref index, out var value, out _);
            index = originalIndex;

            if (searchSucceeded)
            {
                token = value;
                found = parser;
                return true;
            }
        }

        return false;
    }

    bool TryFindMatchingExtendParserBuilder(ref ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out ExtendParserBuilder<TValue>? found, out string? token, out int? tokenPrecedence)
    {
        found = null;
        token = null;
        tokenPrecedence = null;

        foreach (var (kind, precedence, extendParserBuilder) in extendParsers)
        {
            var originalIndex = index;
            bool searchSucceeded = kind(ref input, ref index, out token, out _);
            index = originalIndex;

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
