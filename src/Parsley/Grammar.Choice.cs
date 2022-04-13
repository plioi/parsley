namespace Parsley;

partial class Grammar
{
    /// <summary>
    /// Choice() always fails without consuming input.
    /// 
    /// Choice(p) is equivalent to p.
    /// 
    /// For 2 or more inputs, parsers are applied from left
    /// to right.  If a parser succeeds, its reply is returned.
    /// If a parser fails without consuming input, the next parser
    /// is attempted.  If a parser fails after consuming input,
    /// subsequent parsers will not be attempted.  As long as
    /// parsers consume no input, their error messages are merged.
    ///
    /// Choice is 'predictive' since p[n+1] is only tried when
    /// p[n] didn't consume any input (i.e. the look-ahead is 1).
    /// This non-backtracking behaviour allows for both an efficient
    /// implementation of the parser combinators and the generation
    /// of good error messages.
    /// </summary>
    public static Parser<T> Choice<T>(params Parser<T>[] parsers)
    {
        if (parsers.Length == 0)
            return Grammar<T>.Fail;

        return input =>
        {
            var start = input.Position;
            var reply = parsers[0](input);
            var newPosition = input.Position;

            var errors = ErrorMessageList.Empty;
            var i = 1;

            while (!reply.Success && (start == newPosition) && i < parsers.Length)
            {
                errors = errors.Merge(reply.ErrorMessages);
                reply = parsers[i](input);
                newPosition = input.Position;
                i++;
            }

            if (start == newPosition)
            {
                errors = errors.Merge(reply.ErrorMessages);

                if (reply.Success)
                    reply = new Parsed<T>(reply.Value, reply.Position, errors);
                else
                    reply = new Error<T>(reply.Position, errors);
            }

            return reply;
        };
    }
}
