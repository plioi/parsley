namespace Parsley;

class AssertionException : Exception
{
    public AssertionException(object expected, object actual)
        : base(ExpectationDetails(expected, actual))
    {
    }

    public AssertionException(string message, object expected, object actual)
        : base(ExpectationDetails(expected, actual) + Environment.NewLine + Environment.NewLine + message)
    {
    }

    static string ExpectationDetails(object expected, object actual)
        => $"{Environment.NewLine}Expected: {expected}{Environment.NewLine}But was:  {actual}";
}
