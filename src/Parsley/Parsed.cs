using System;

namespace Parsley
{
    public class Parsed<T> : Reply<T>
    {
        private readonly ErrorMessageList potentialErrors;

        public Parsed(T value, Lexer unparsedTokens)
            :this(value, unparsedTokens, ErrorMessageList.Empty) { }

        public Parsed(T value, Lexer unparsedTokens, ErrorMessageList potentialErrors)
        {
            Value = value;
            UnparsedTokens = unparsedTokens;
            this.potentialErrors = potentialErrors;
        }

        public T Value { get; private set; }
        public Lexer UnparsedTokens { get; private set; }
        public bool Success { get { return true; } }
        public ErrorMessageList ErrorMessages { get { return potentialErrors; } }
    }
}