namespace Parsley;

public delegate IParser<T> ExtendParserBuilder<T>(T left);
public delegate T AtomNodeBuilder<out T>(Token atom);
public delegate T UnaryNodeBuilder<T>(Token symbol, T operand);
public delegate T BinaryNodeBuilder<T>(T left, Token symbol, T right);
public enum Associativity { Left, Right }

public class OperatorPrecedenceParser<T> : IParser<T>
{
    readonly List<(IParser<Token>, IParser<T>)> unitParsers = new();
    readonly List<(IParser<Token>, int precedence, ExtendParserBuilder<T>)> extendParsers = new();

    public void Unit(IParser<Token> kind, IParser<T> unitParser)
    {
        unitParsers.Add((kind, unitParser));
    }

    public void Atom(IParser<Token> kind, AtomNodeBuilder<T> createAtomNode)
    {
        Unit(kind, from token in kind
            select createAtomNode(token));
    }

    public void Prefix(IParser<Token> operation, int precedence, UnaryNodeBuilder<T> createUnaryNode)
    {
        Unit(operation, from symbol in operation
            from operand in OperandAtPrecedenceLevel(precedence)
            select createUnaryNode(symbol, operand));
    }

    public void Extend(IParser<Token> operation, int precedence, ExtendParserBuilder<T> createExtendParser)
    {
        extendParsers.Add((operation, precedence, createExtendParser));
    }

    public void Postfix(IParser<Token> operation, int precedence, UnaryNodeBuilder<T> createUnaryNode)
    {
        Extend(operation, precedence, left => from symbol in operation
            select createUnaryNode(symbol, left));
    }

    public void Binary(IParser<Token> operation, int precedence, BinaryNodeBuilder<T> createBinaryNode,
                       Associativity associativity = Associativity.Left)
    {
        int rightOperandPrecedence = precedence;

        if (associativity == Associativity.Right)
            rightOperandPrecedence = precedence - 1;

        Extend(operation, precedence, left => from symbol in operation
            from right in OperandAtPrecedenceLevel(rightOperandPrecedence)
            select createBinaryNode(left, symbol, right));
    }

    public Reply<T> Parse(Text input)
    {
        return Parse(input, 0);
    }

    IParser<T> OperandAtPrecedenceLevel(int precedence)
    {
        return new LambdaParser<T>(input => Parse(input, precedence));
    }

    Reply<T> Parse(Text input, int precedence)
    {
        var matchingUnitParser = FirstMatchingUnitParserOrNull(input, out var token);

        if (matchingUnitParser == null)
            return new Error<T>(input, ErrorMessage.Unknown());

        var reply = matchingUnitParser.Parse(input);

        if (!reply.Success)
            return reply;

        input = reply.UnparsedInput;

        var matchingExtendParserBuilder = FirstMatchingExtendParserBuilderOrNull(input, out token, out int? tokenPrecedence);

        while (matchingExtendParserBuilder != null && precedence < tokenPrecedence)
        {
            //Continue parsing at this precedence level.

            var extendParser = matchingExtendParserBuilder(reply.Value);

            reply = extendParser.Parse(input);

            if (!reply.Success)
                return reply;

            input = reply.UnparsedInput;

            matchingExtendParserBuilder = FirstMatchingExtendParserBuilderOrNull(input, out token, out tokenPrecedence);
        }

        return reply;
    }

    IParser<T> FirstMatchingUnitParserOrNull(Text input, out Token token)
    {
        token = null;

        foreach(var (kind, parser) in unitParsers)
        {
            var reply = kind.Parse(input);

            if (reply.Success)
            {
                token = reply.Value;
                return parser;
            }
        }

        return null;
    }

    ExtendParserBuilder<T> FirstMatchingExtendParserBuilderOrNull(Text input, out Token token, out int? tokenPrecedence)
    {
        token = null;
        tokenPrecedence = null;

        foreach (var (kind, precedence, extendParserBuilder) in extendParsers)
        {
            var reply = kind.Parse(input);

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
