using System;

namespace Parsley
{
    public class Error<T> : Reply<T>
    {
        private readonly ErrorMessageList errors;

        public Error(TokenStream unparsedTokens, ErrorMessage error)
            : this(unparsedTokens,  ErrorMessageList.Empty.With(error)) { }

        public Error(TokenStream unparsedTokens, ErrorMessageList errors)
        {
            UnparsedTokens = unparsedTokens;

            this.errors = errors;
        }

        public T Value
        {
            get
            {
                var position = UnparsedTokens.Position;
                throw new MemberAccessException(String.Format("{0}: {1}", position, ErrorMessages));
            }
        }

        public TokenStream UnparsedTokens { get; private set; }

        public bool Success { get { return false; } }

        public ErrorMessageList ErrorMessages
        {
            get { return errors; }
        }
    }
}