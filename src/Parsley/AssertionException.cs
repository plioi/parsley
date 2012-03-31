using System;

namespace Parsley
{
    internal class AssertionException : Exception
    {
        public AssertionException(object expected, object actual)
            : base(ExpectationDetails(expected, actual))
        {
        }

        public AssertionException(string message, object expected, object actual)
            : base(Environment.NewLine + message + ExpectationDetails(expected, actual))
        {
        }

        private static string ExpectationDetails(object expected, object actual)
        {
            return string.Format(Environment.NewLine + "Expected: {0}" +
                                 Environment.NewLine + "But was:  {1}",
                                 expected, actual);
        }
    }
}