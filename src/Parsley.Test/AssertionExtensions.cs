using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Parsley
{
    public static class AssertionExtensions
    {
        public static void ShouldBeFalse(this bool condition)
        {
            Assert.IsFalse(condition);
        }

        public static void ShouldBeTrue(this bool condition)
        {
            Assert.IsTrue(condition);
        }

        public static void ShouldBeTrue(this bool condition, string message)
        {
            Assert.IsTrue(condition, message);
        }

        public static void ShouldBeFalse(this bool condition, string message)
        {
            Assert.IsFalse(condition, message);
        }

        public static void ShouldEqual(this object actual, object expected)
        {
            Assert.AreEqual(expected, actual);
        }

        public static void ShouldNotEqual(this object actual, object expected)
        {
            Assert.AreNotEqual(expected, actual);
        }

        public static void ShouldBeNull(this object o)
        {
            Assert.IsNull(o);
        }

        public static void ShouldBeNull(this object o, string message)
        {
            Assert.IsNull(o, message);
        }

        public static void ShouldNotBeNull(this object o)
        {
            Assert.IsNotNull(o);
        }

        public static void ShouldBeTheSameAs(this object actual, object expected)
        {
            Assert.AreSame(expected, actual);
        }

        public static void ShouldNotBeTheSameAs(this object actual, object expected)
        {
            Assert.AreNotSame(expected, actual);
        }

        public static void ShouldBeEmpty<T>(this IEnumerable<T> actual)
        {
            Assert.IsEmpty(actual.ToArray());
        }

        public static void ShouldBeInstanceOf<T>(this object actual)
        {
            Assert.IsInstanceOf<T>(actual);
        }

        public static void ShouldList<T>(this IEnumerable<T> actual, params T[] expected)
        {
            actual.ToArray().ShouldEqual(expected);
        }

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

            threw.ShouldBeTrue(String.Format("Expected {0}.", typeof(TException).Name));
        }
    }
}
