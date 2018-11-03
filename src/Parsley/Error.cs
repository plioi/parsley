using System;

namespace Parsley
{
    public class Error<T> : ErrorGeneral, IReply<T>
    {
        public Error(TokenStream unparsedTokens, ErrorMessage error)
            : base(unparsedTokens, error)
        { }

        public Error(TokenStream unparsedTokens, ErrorMessageList errors)
            : base(unparsedTokens, errors)
        { }

        public new static Error<T> From(IGeneralReply r)
        {
            return new Error<T>(r.UnparsedTokens, r.ErrorMessages);
        }

        public T Value => throw new MemberAccessException($"{UnparsedTokens.Position}: {ErrorMessages}");
    }
}