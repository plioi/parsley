using System;

namespace Parsley.Test.IntegrationTests.Calculator
{
    public interface Expression
    {
        int Value { get; }
    }

    public class ConstantExpression : Expression
    {
        public ConstantExpression(int value)
        {
            Value = value;
        }

        public int Value { get; private set; }
    }

    public class BinaryExpression : Expression
    {
        private readonly Expression left;
        private readonly Expression right;
        private readonly Func<int, int, int> operation;

        public BinaryExpression(Expression left, Expression right, Func<int, int, int> operation)
        {
            this.left = left;
            this.operation = operation;
            this.right = right;
        }

        public int Value
        {
            get { return operation(left.Value, right.Value); }
        }
    }
}
