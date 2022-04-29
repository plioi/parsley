using System.Diagnostics.CodeAnalysis;

namespace Parsley;

public delegate Parser_char_<TValue> ExtendParserBuilder<TValue>(TValue left);
public delegate TValue AtomNodeBuilder<out TValue>(string atom);
public delegate TValue UnaryNodeBuilder<TValue>(string symbol, TValue operand);
public delegate TValue BinaryNodeBuilder<TValue>(TValue left, string symbol, TValue right);
public enum Associativity { Left, Right }

public class OperatorPrecedenceParser<TValue>
{
    readonly List<(Parser_char_<string>, Parser_char_<TValue>)> unitParsers = new();
    readonly List<(Parser_char_<string>, int precedence, ExtendParserBuilder<TValue>)> extendParsers = new();

    public void Unit(Parser_char_<string> kind, Parser_char_<TValue> unitParser)
    {
        unitParsers.Add((kind, unitParser));
    }

    public void Atom(Parser_char_<string> kind, AtomNodeBuilder<TValue> createAtomNode)
    {
        Unit(kind, from token in kind
            select createAtomNode(token));
    }

    public void Prefix(Parser_char_<string> operation, int precedence, UnaryNodeBuilder<TValue> createUnaryNode)
    {
        Unit(operation, from symbol in operation
            from operand in OperandAtPrecedenceLevel(precedence)
            select createUnaryNode(symbol, operand));
    }

    public void Extend(Parser_char_<string> operation, int precedence, ExtendParserBuilder<TValue> createExtendParser)
    {
        extendParsers.Add((operation, precedence, createExtendParser));
    }

    public void Postfix(Parser_char_<string> operation, int precedence, UnaryNodeBuilder<TValue> createUnaryNode)
    {
        Extend(operation, precedence, left => from symbol in operation
            select createUnaryNode(symbol, left));
    }

    public void Binary(Parser_char_<string> operation, int precedence, BinaryNodeBuilder<TValue> createBinaryNode,
                       Associativity associativity = Associativity.Left)
    {
        int rightOperandPrecedence = precedence;

        if (associativity == Associativity.Right)
            rightOperandPrecedence = precedence - 1;

        Extend(operation, precedence, left => from symbol in operation
            from right in OperandAtPrecedenceLevel(rightOperandPrecedence)
            select createBinaryNode(left, symbol, right));
    }

    public Parser_char_<TValue> Parser
        => (ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out TValue? value, [NotNullWhen(false)] out string? expectation)
                => Parse(input, ref index, 0, out value, out expectation);

    Parser_char_<TValue> OperandAtPrecedenceLevel(int precedence)
        => (ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out TValue? value, [NotNullWhen(false)] out string? expectation)
            => Parse(input, ref index, precedence, out value, out expectation);

    bool Parse(ReadOnlySpan<char> input, ref int index, int precedence, [NotNullWhen(true)] out TValue? value, [NotNullWhen(false)] out string? expectation)
    {
        if (!TryFindMatchingUnitParser(input, ref index, out var matchingUnitParser, out var token))
        {
            expectation = "expression";
            value = default;
            return false;
        }

        if (matchingUnitParser(input, ref index, out value, out expectation))
        {
            while (TryFindMatchingExtendParserBuilder(input, ref index, out var matchingExtendParserBuilder, out token, out int? tokenPrecedence) && precedence < tokenPrecedence)
            {
                //Continue parsing at this precedence level.

                var extendParser = matchingExtendParserBuilder(value);

                if (extendParser(input, ref index, out value, out expectation))
                    continue;

                return false;
            }

            return true;
        }

        return false;
    }

    bool TryFindMatchingUnitParser(ReadOnlySpan<char> input, ref int index, [NotNullWhen(true)] out Parser_char_<TValue>? found, out string? token)
    {
        found = null;
        token = null;

        foreach(var (kind, parser) in unitParsers)
        {
            var originalIndex = index;
            bool searchSucceeded = kind(input, ref index, out var value, out _);
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
            bool searchSucceeded = kind(input, ref index, out token, out _);
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
