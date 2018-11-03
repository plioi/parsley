﻿namespace Parsley
{
    public class Parsed<T> : ParsedGeneral, IReply<T>
    {
        public Parsed(T value, TokenStream unparsedTokens)
            : this(value, unparsedTokens, ErrorMessageList.Empty)
        { }

        public Parsed(T value, TokenStream unparsedTokens, ErrorMessageList potentialErrors)
            : base (unparsedTokens)
        {
            Value = value;
            ErrorMessages = potentialErrors;
        }

        public T Value { get; }
        public override ErrorMessageList ErrorMessages { get; }
    }
}