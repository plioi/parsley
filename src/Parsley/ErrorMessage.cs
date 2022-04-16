namespace Parsley;

public abstract class ErrorMessage
{
    public static readonly ErrorMessage Unknown = new UnknownErrorMessage();

    public static ErrorMessage Expected(string expectation)
        => new ExpectedErrorMessage(expectation);

    public static ErrorMessage Backtrack(Position position, ErrorMessageList errors)
        => new BacktrackErrorMessage(position, errors);
}

public class UnknownErrorMessage : ErrorMessage
{
    internal UnknownErrorMessage() { }
}

/// <summary>
/// Parsers report this when a specific expectation was not met at the current position.
/// </summary>
public class ExpectedErrorMessage : ErrorMessage
{
    internal ExpectedErrorMessage(string expectation)
    {
        Expectation = expectation;
    }

    public string Expectation { get; }

    public override string ToString()
        => Expectation + " expected";
}

/// <summary>
/// Parsers report this when they have backtracked after an error occurred.
/// The Position property describes the position where the original error
/// occurred.
/// </summary>
public class BacktrackErrorMessage : ErrorMessage
{
    internal BacktrackErrorMessage(Position position, ErrorMessageList errors)
    {
        Position = position;
        Errors = errors;
    }

    public Position Position { get; }
    public ErrorMessageList Errors { get; }

    public override string ToString()
        => $"{Position}: {Errors}";
}
