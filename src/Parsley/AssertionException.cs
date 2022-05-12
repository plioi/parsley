using static System.Environment;

namespace Parsley;

public class AssertionException : Exception
{
    public AssertionException(object expected, object actual)
        : base(ExpectationDetails(expected, actual))
    {
    }

    public AssertionException(string message, object expected, object actual)
        : base(ExpectationDetails(expected, actual) + NewLine + NewLine + message)
    {
    }

    static string ExpectationDetails(object expected, object actual)
        => $"{NewLine}Expected: {expected}{NewLine}But was:  {actual}";
}

public class MessageAssertionException : Exception
{
    public MessageAssertionException(string expectedMessage, string actualMessage)
        : base(ExpectationDetails(expectedMessage, actualMessage))
    {
    }
    
    static string ExpectationDetails(string expectedMessage, string actualMessage)
        => $"{NewLine}Expected message:{NewLine}\t{expectedMessage}{NewLine}But was message:{NewLine}\t{actualMessage}";
}

