using System;
using System.Collections.Generic;
using Should;
using Xunit;

namespace Parsley
{
    public class StreamTests
    {
        [Fact]
        public void RequiresAtLeastOneItem()
        {
            Action constructStreamOverEmptyCollection = () => new Stream<int>(Empty());

            constructStreamOverEmptyCollection.ShouldThrow<ArgumentException>("Streams may not be empty.");
        }

        [Fact]
        public void TraversesUnderlyingEnumerator()
        {
            var stream = new Stream<int>(OneTwoThree());
            stream.Current.ShouldEqual(1);

            stream = stream.Advance();
            stream.Current.ShouldEqual(2);

            stream = stream.Advance();
            stream.Current.ShouldEqual(3);
        }

        [Fact]
        public void DoesNotChangeStateAsUnderlyingEnumeratorIsTraversed()
        {
            var one = new Stream<int>(OneTwoThree());
            one.Current.ShouldEqual(1);

            var two = one.Advance();
            one.Current.ShouldEqual(1);
            two.Current.ShouldEqual(2);

            var three = two.Advance();
            one.Current.ShouldEqual(1);
            two.Current.ShouldEqual(2);
            three.Current.ShouldEqual(3);
        }

        [Fact]
        public void AllowsRepeatableTraversalWhileTraversingUnderlyingEnumeratorItemsAtMostOnce()
        {
            var one = new Stream<int>(OneTwoThree());
            one.Current.ShouldEqual(1);
            one.Advance().Current.ShouldEqual(2);
            one.Advance().Advance().Current.ShouldEqual(3);

            one.Advance().ShouldBeSameAs(one.Advance());
        }

        [Fact]
        public void TryingToAdvanceBeyondFinalItemResultsInNoMovement()
        {
            var one = new Stream<int>(OneTwoThree());
            var two = one.Advance();
            var three = two.Advance();

            three.Advance().ShouldBeSameAs(three);
            three.Advance().Advance().ShouldBeSameAs(three);
        }

        private static IEnumerator<int> Empty()
        {
            yield break;
        }

        private static IEnumerator<int> OneTwoThree()
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }
    }
}
