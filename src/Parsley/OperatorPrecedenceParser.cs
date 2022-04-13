#nullable disable
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

    public Parser<T> Parser => input => Parse(input, 0);

    Parser<T> OperandAtPrecedenceLevel(int precedence)
    {
        return input => Parse(input, precedence);
    }

    Reply<T> Parse(Text input, int precedence)
    {
        var matchingUnitParser = FirstMatchingUnitParserOrNull(input, out var token);

        if (matchingUnitParser == null)
            return new Error<T>(input.Position, ErrorMessage.Unknown());

        var reply = matchingUnitParser(input);

        if (!reply.Success)
            return reply;

        var matchingExtendParserBuilder = FirstMatchingExtendParserBuilderOrNull(input, out token, out int? tokenPrecedence);

        while (matchingExtendParserBuilder != null && precedence < tokenPrecedence)
        {
            //Continue parsing at this precedence level.

            var extendParser = matchingExtendParserBuilder(reply.Value);

            reply = extendParser(input);

            if (!reply.Success)
                return reply;

            matchingExtendParserBuilder = FirstMatchingExtendParserBuilderOrNull(input, out token, out tokenPrecedence);
        }

        return reply;
    }

    Parser<T> FirstMatchingUnitParserOrNull(Text input, out string token)
    {
        token = null;

        foreach(var (kind, parser) in unitParsers)
        {
            var snapshot = input.Snapshot();
            var reply = kind(input);
            input.Restore(snapshot);

            if (reply.Success)
            {
                token = reply.Value;
                return parser;
            }
        }

        return null;
    }

    ExtendParserBuilder<T> FirstMatchingExtendParserBuilderOrNull(Text input, out string token, out int? tokenPrecedence)
    {
        token = null;
        tokenPrecedence = null;

        foreach (var (kind, precedence, extendParserBuilder) in extendParsers)
        {
            var snapshot = input.Snapshot();
            var reply = kind(input);
            input.Restore(snapshot);

            if (reply.Success)
            {
                token = reply.Value;
                tokenPrecedence = precedence;
                return extendParserBuilder;
            }
        }

        return null;
    }
}
