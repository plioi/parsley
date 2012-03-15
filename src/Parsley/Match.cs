namespace Parsley
{
    public class Match
    {
        public Match(bool success, string value)
        {
            Success = success;
            Value = value;
        }

        public bool Success { get; private set; }
        public string Value { get; private set; }
    }
}