namespace Parsley;

public class MatchResult
{
    public static readonly MatchResult Fail = new MatchResult(false, "");

    public static MatchResult Succeed(string value) => new MatchResult(true, value);

    MatchResult(bool success, string value)
    {
        Success = success;
        Value = value;
    }

    public bool Success { get; }
    public string Value { get; }
}
