namespace Parsley;

public class MatchResult
{
    public static readonly MatchResult Fail = new(false, "");

    public static MatchResult Succeed(string value) => new(true, value);

    MatchResult(bool success, string value)
    {
        Success = success;
        Value = value;
    }

    public bool Success { get; }
    public string Value { get; }
}
