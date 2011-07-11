using System;

namespace Parsley
{
    public class Error<T> : Reply<T>
    {
        private readonly ErrorMessageList errors;

        public Error(Lexer unparsedTokens, ErrorMessage error)
            : this(unparsedTokens,  ErrorMessageList.Empty.With(error)) { }

        public Error(Lexer unparsedTokens, ErrorMessageList errors)
        {
            UnparsedTokens = unparsedTokens;

            this.errors = errors;
        }

        public T Value
        {
            get
            {
                Position position = UnparsedTokens.Position;
                throw new MemberAccessException(String.Format("({0}, {1}): {2}", position.Line, position.Column, ErrorMessages));
            }
        }

        public Lexer UnparsedTokens { get; private set; }

        public bool Success { get { return false; } }

        public ErrorMessageList ErrorMessages
        {
            get { return errors; }
        }

        public Reply<U> ParseRest<U>(Func<T, Parser<U>> constructNextParser)
        {
            return new Error<U>(UnparsedTokens, errors);
        }
    }
}