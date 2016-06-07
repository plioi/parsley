namespace Parsley
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ErrorMessageList
    {
        public static readonly ErrorMessageList Empty = new ErrorMessageList();

        private readonly ErrorMessage head;
        private readonly ErrorMessageList tail;

        private ErrorMessageList()
        {
            head = null;
            tail = null;
        }

        private ErrorMessageList(ErrorMessage head, ErrorMessageList tail)
        {
            this.head = head;
            this.tail = tail;
        }

        public ErrorMessageList With(ErrorMessage errorMessage)
        {
            return new ErrorMessageList(errorMessage, this);
        }

        public ErrorMessageList Merge(ErrorMessageList errors)
        {
            var result = this;

            foreach (var error in errors.All<ErrorMessage>())
                result = result.With(error);

            return result;
        }

        public override string ToString()
        {
            var undefinedGrammarRule = All<UndefinedGrammarRuleErrorMessage>().FirstOrDefault();
            if (undefinedGrammarRule != null)
                return undefinedGrammarRule.ToString();

            var expectationErrors = new List<string>(All<ExpectedErrorMessage>()
                                              .Select(error => error.Expectation)
                                              .Distinct()
                                              .OrderBy(expectation => expectation));

            var backtrackErrors = All<BacktrackErrorMessage>().ToArray();

            if (!expectationErrors.Any() && !backtrackErrors.Any())
            {
                var unknownError = All<UnknownErrorMessage>().FirstOrDefault();
                if (unknownError != null)
                    return unknownError.ToString();

                return "";
            }

            var parts = new List<string>();

            if (expectationErrors.Any())
            {
                var suffixes = Separators(expectationErrors.Count - 1).Concat(new[] { " expected" });

                parts.Add(String.Join("", expectationErrors.Zip(suffixes, (error, suffix) => error + suffix)));
            }

            if (backtrackErrors.Any())
                parts.Add(String.Join(" ", backtrackErrors.Select(backtrack => String.Format("[{0}]", backtrack))));

            return String.Join(" ", parts);
        }

        private static IEnumerable<string> Separators(int count)
        {
            if (count <= 0)
                return Enumerable.Empty<string>();
            return Enumerable.Repeat(", ", count - 1).Concat(new[] { " or " });
        }

        private IEnumerable<T> All<T>() where T : ErrorMessage
        {
            if (this != Empty)
            {
                if (head is T)
                    yield return (T)head;
                foreach (T message in tail.All<T>())
                        yield return message;
            }
        }
    }
}