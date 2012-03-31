namespace Parsley
{
    public class MatchResult
    {
        public static readonly MatchResult Fail = new MatchResult(false, "");

        public static MatchResult Succeed(string value)
        {
            return new MatchResult(true, value);
        }

        private MatchResult(bool success, string value)
        {
            Success = success;
            Value = value;
        }

        public bool Success { get; private set; }
        public string Value { get; private set; }
    }
}