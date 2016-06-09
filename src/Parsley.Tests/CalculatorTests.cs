namespace Parsley.Tests
{
    using Xunit;
    using Shouldly;

    public class CalculatorTests
    {
        [Fact]
        public void PassingTest()
        {
            Calculator.Add(2, 2).ShouldBe(4);
        }

        [Fact]
        public void FailingTest()
        {
            Calculator.Add(2, 2).ShouldBe(5);
        }
    }
}