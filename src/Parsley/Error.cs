namespace Parsley
{
    using System;

    public class Error<T> : Reply<T>
    {
        public Error(TokenStream unparsedTokens, ErrorMessage error)
            : this(unparsedTokens,  ErrorMessageList.Empty.With(error)) { }

        public Error(TokenStream unparsedTokens, ErrorMessageList errors)
        {
            UnparsedTokens = unparsedTokens;
            ErrorMessages = errors;
        }

        public T Value
        {
            get
            {
                var position = UnparsedTokens.Position;
                throw new MemberAccessException($"{position}: {ErrorMessages}");
            }
        }

        public TokenStream UnparsedTokens { get; }
        public bool Success => false;
        public ErrorMessageList ErrorMessages { get; }
    }
}