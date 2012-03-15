namespace Parsley
{
    public class MatchResult
    {
        public static MatchResult Succeed(string value)
        {
            return new MatchResult(true, value);
        }

        public static MatchResult Fail()
        {
            return new MatchResult(false, "");
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