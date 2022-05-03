using System.Diagnostics.CodeAnalysis;

namespace Parsley;

public delegate Parser<char, TValue> ExtendParserBuilder<TValue>(TValue left);
public delegate TValue AtomNodeBuilder<out TValue>(string atom);
public delegate TValue UnaryNodeBuilder<TValue>(string symbol, TValue operand);
public delegate TValue BinaryNodeBuilder<TValue>(TValue left, string symbol, TValue right);
public enum Associativity { Left, Right }

public class OperatorPrecedenceParser<TValue>
{
    readonly List<(Parser<char, string>, Parser<char, TValue>)> unitParsers = new();
    readonly List<(Parser<char, string>, int precedence, ExtendParserBuilder<TValue>)> extendParsers = new();

    public void Unit(Parser<char, string> kind, Parser<char, TValue> unitParser)
    {
        unitParsers.Add((kind, unitParser));
    }

    public void Atom(Parser<char, string> kind, AtomNodeBuilder<TValue> createAtomNode)
    {
        Unit(kind, from token in kind
            select createAtomNode(token));
    }

    public void Prefix(Parser<char, string> operation, int precedence, UnaryNodeBuilder<TValue> createUnaryNode)
    {
        Unit(operation, from symbol in operation
            from operand in OperandAtPrecedenceLevel(precedence)
            select createUnaryNode(symbol, operand));
    }

    public void Extend(Parser<char, string> operation, int precedence, ExtendParserBuilder<TValue> createExtendParser)
    {
        extendParsers.Add((operation, precedence, createExtendParser));
    }

    public void Postfix(Parser<char, string> operation, int precedence, UnaryNodeBuilder<TValue> createUnaryNode)
    {
        Extend(operation, precedence, left => from symbol in operation
            select createUnaryNode(symbol, left));
    }

    public void Binary(Parser<char, string> operation, int precedence, BinaryNodeBuilder<TValue> createBinaryNode,
                       Associativity associativity = Associativity.Left)
    {
        int rightOperandPrecedence = precedence;

        if (associativity == Associativity.Right)
            rightOperandPrecedence = precedence - 1;

        Extend(operation, precedence, left => from symbol in operation
            from right in OperandAtPrecedenceLevel(rightOperandPrecedence)
            select createBinaryNode(left, symbol, right));
    }

    public Parser<char, TValue> Parser
        => (ReadOnlySpan<char> input, ref int index, out bool succeeded, out string? expectation)
                => Parse(input, ref index, 0, out succeeded, out expectation);

    Parser<char, TValue> OperandAtPrecedenceLevel(int precedence)
        => (ReadOnlySpan<char> input, ref int index, out bool succeeded, out string? expectation)
            => Parse(input, ref index, precedence, out succeeded, out expectation);

    TValue? Parse(ReadOnlySpan<char> input, ref int index, int precedence, out bool succeeded, out string? expectation)
    {
        if (!TryFindMatchingUnitParser(input, ref index, out var matchingUnitParser, out var token))
        {
            expectation = "expression";
            succeeded = false;
            return default;
        }

        var value = matchingUnitParser(input, ref index, out var matchingUnitParseSucceeded, out expectation);

        if (matchingUnitParseSucceeded)
        {
            while (TryFindMatchingExtendParserBuilder(input, ref index, out var matchingExtendParserBuilder, out token, out int? tokenPrecedence) && precedence < tokenPrecedence)
            {
                //Continue parsing at this precedence level.

                var extendParser = matchingExtendParserBuilder(value!);

                value = extendParser(input, ref index, out var extendParseSucceeded, out expectation);

                if (extendParseSucceeded)
                    continue;

                succeeded = false;
                return value;
            }

            succeeded = true;
            return value;
        }

        succeeded = false;
        return value;
    }

    bool TryFindMatchingUnitParser(ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out Parser<char, TValue>? found, out string? token)
    {
        found = null;
        token = null;

        foreach(var (kind, parser) in unitParsers)
        {
            var originalIndex = index;
            var value = kind(input, ref index, out var searchSucceeded, out _);
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

    bool TryFindMatchingExtendParserBuilder(ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out ExtendParserBuilder<TValue>? found, out string? token, out int? tokenPrecedence)
    {
        found = null;
        token = null;
        tokenPrecedence = null;

        foreach (var (kind, precedence, extendParserBuilder) in extendParsers)
        {
            var originalIndex = index;
            token = kind(input, ref index, out var searchSucceeded, out _);
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
