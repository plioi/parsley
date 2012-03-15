using System;
using Should;

namespace Parsley
{
    public static class AssertionExtensions
    {
        public static void ShouldThrow<TException>(this Action shouldThrow, string expectedMessage) where TException : Exception
        {
            bool threw = false;

            try
            {
                shouldThrow();
            }
            catch (TException ex)
            {
                ex.Message.ShouldEqual(expectedMessage);
                threw = true;
            }

            threw.ShouldBeTrue(String.Format("Expected {0}.", typeof (TException).Name));
        }

        public static void ShouldThrow<TException>(this Func<object> shouldThrow, string expectedMessage) where TException : Exception
        {
            Action action = () => shouldThrow();
            action.ShouldThrow<TException>(expectedMessage);
        }

        public static void ShouldSucceed(this Match actual, string expected)
        {
            actual.Success.ShouldBeTrue();
            actual.Value.ShouldEqual(expected);
        }

        public static void ShouldFail(this Match actual)
        {
            actual.Success.ShouldBeFalse();
            actual.Value.ShouldEqual("");
        }
    }
}