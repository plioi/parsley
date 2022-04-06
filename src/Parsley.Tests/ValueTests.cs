namespace Parsley.Tests
{
    using Shouldly;

    public class ValueTests
    {
        public void AnInstanceShouldAlwaysReturnTheSameHashCode()
        {
            var o = new Sample(1, "A");
            o.GetHashCode().ShouldBe(o.GetHashCode());
        }

        public void HashCodesOfEquivalentObjectsShouldBeEqual()
        {
            var a = new Sample(1, "A");
            var b = new Sample(1, "A");
            a.GetHashCode().ShouldBe(b.GetHashCode());
        }

        public void HashCodesOfNonEquivalentObjectsShouldUsuallyBeDifferent()
        {
            var a = new Sample(0, "A");
            var b = new Sample(1, "A");
            var c = new Sample(0, "B");
            a.GetHashCode().ShouldNotBe(b.GetHashCode());
            a.GetHashCode().ShouldNotBe(c.GetHashCode());
            b.GetHashCode().ShouldNotBe(c.GetHashCode());
        }

        public void HashCodesShouldBeSafeFromNullFields()
        {
            new Sample(0, null).GetHashCode();
        }

        public void HashCodesShouldBeSafeFromNumericOverflow()
        {
            new Sample(int.MaxValue, "A").GetHashCode();
        }

        public void IsEquatableByComparingImmutableFields()
        {
            var nil = (Sample)null;
            new Sample(0, "A").Equals(new Sample(0, "A")).ShouldBeTrue();
            new Sample(0, "A").Equals(nil).ShouldBeFalse();
            new Sample(0, "A").Equals(new Sample(1, "A")).ShouldBeFalse();
            new Sample(0, "A").Equals(new Sample(0, null)).ShouldBeFalse();
            new Sample(0, null).Equals(new Sample(0, "A")).ShouldBeFalse();
        }

        public void OverridesObjectEquals()
        {
            object nil = null;
            new Sample(0, "A").Equals((object)new Sample(0, "A")).ShouldBeTrue();
            new Sample(0, "A").Equals(nil).ShouldBeFalse();
            new Sample(0, "A").Equals((object)new Sample(1, "A")).ShouldBeFalse();
            new Sample(0, "A").Equals((object)new Sample(0, null)).ShouldBeFalse();
            new Sample(0, null).Equals((object)new Sample(0, "A")).ShouldBeFalse();
        }

        public void OverloadsEqualityOperators()
        {
            var a = new Sample(0, "A");
            var a2 = new Sample(0, "A");
            var b = new Sample(0, "B");

            #pragma warning disable 1718
            (a == a).ShouldBeTrue();
            #pragma warning restore 1718

            (a == b).ShouldBeFalse();
            (a == a2).ShouldBeTrue();
            (null == a).ShouldBeFalse();
            (a == null).ShouldBeFalse();

            #pragma warning disable 1718
            (a != a).ShouldBeFalse();
            #pragma warning restore 1718

            (a != b).ShouldBeTrue();
            (a != a2).ShouldBeFalse();
            (null != a).ShouldBeTrue();
            (a != null).ShouldBeTrue();
        }

        class Sample : Value<Sample>
        {
            readonly int i;
            readonly string s;

            public Sample(int i, string s)
            {
                this.i = i;
                this.s = s;
            }

            protected override object[] ImmutableFields() => new object[] { i, s };
        }
    }
}
