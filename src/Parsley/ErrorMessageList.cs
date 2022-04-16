namespace Parsley;

public class ErrorMessageList
{
    public static readonly ErrorMessageList Empty = new();

    readonly ErrorMessage? head;
    readonly ErrorMessageList? tail;

    ErrorMessageList()
    {
        head = null;
        tail = null;
    }

    ErrorMessageList(ErrorMessage head, ErrorMessageList tail)
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
        var expectationErrors = new List<string>(All<ExpectedErrorMessage>()
            .Select(error => error.Expectation)
            .Distinct()
            .OrderBy(expectation => expectation));

        var backtrackErrors = All<BacktrackErrorMessage>().ToArray();

        if (!expectationErrors.Any() && !backtrackErrors.Any())
            return All<UnknownErrorMessage>().Any() ? "Parse error." : "";

        var parts = new List<string>();

        if (expectationErrors.Any())
        {
            var suffixes = Separators(expectationErrors.Count - 1).Concat(new[] { " expected" });

            parts.Add(string.Join("", expectationErrors.Zip(suffixes, (error, suffix) => error + suffix)));
        }

        if (backtrackErrors.Any())
            parts.Add(string.Join(" ", backtrackErrors.Select(backtrack => $"[{backtrack.Position}: {backtrack.Errors}]")));

        return string.Join(" ", parts);
    }

    static IEnumerable<string> Separators(int count)
    {
        if (count <= 0)
            return Enumerable.Empty<string>();
        return Enumerable.Repeat(", ", count - 1).Concat(new[] { " or " });
    }

    IEnumerable<T> All<T>() where T : ErrorMessage
    {
        if (head is T match)
            yield return match;

        if (tail != null)
            foreach (var message in tail.All<T>())
                yield return message;
    }
}
