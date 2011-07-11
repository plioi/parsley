using NUnit.Framework;

namespace Parsley
{
    [TestFixture]
    public class ErrorMessageListTests
    {
        [Test]
        public void ShouldProvideSharedEmptyInstance()
        {
            ErrorMessageList.Empty.ShouldBeTheSameAs(ErrorMessageList.Empty);
        }

        [Test]
        public void CanBeEmpty()
        {
            ErrorMessageList.Empty.ToString().ShouldEqual("");
        }

        [Test]
        public void CreatesNewCollectionWhenAddingItems()
        {
            ErrorMessageList list = ErrorMessageList.Empty.With(ErrorMessage.Expected("expectation"));

            list.ToString().ShouldEqual("expectation expected");
            list.ShouldNotBeTheSameAs(ErrorMessageList.Empty);
        }

        [Test]
        public void CanIncludeUnknownErrors()
        {
            ErrorMessageList.Empty
                .With(ErrorMessage.Unknown())
                .ToString().ShouldEqual("Parse error.");
        }

        [Test]
        public void CanIncludeMultipleExpectations()
        {
            ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"))
                .ToString().ShouldEqual("A or B expected");

            ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"))
                .With(ErrorMessage.Expected("C"))
                .ToString().ShouldEqual("A, B or C expected");

            ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"))
                .With(ErrorMessage.Expected("C"))
                .With(ErrorMessage.Expected("D"))
                .ToString().ShouldEqual("A, B, C or D expected");
        }

        [Test]
        public void OmitsDuplicateExpectationsFromExpectationLists()
        {
            ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"))
                .With(ErrorMessage.Expected("C"))
                .With(ErrorMessage.Unknown())
                .With(ErrorMessage.Expected("C"))
                .With(ErrorMessage.Expected("C"))
                .With(ErrorMessage.Expected("A"))
                .ToString().ShouldEqual("A, B or C expected");
        }

        [Test]
        public void CanIncludeBacktrackErrors()
        {
            var deepBacktrack = ErrorMessage.Backtrack(new Position(3, 4),
                                                       ErrorMessageList.Empty
                                                           .With(ErrorMessage.Expected("A"))
                                                           .With(ErrorMessage.Expected("B")));

            var shallowBacktrack = ErrorMessage.Backtrack(new Position(2, 3),
                                                          ErrorMessageList.Empty
                                                              .With(ErrorMessage.Expected("C"))
                                                              .With(ErrorMessage.Expected("D"))
                                                              .With(deepBacktrack));
            
            var unrelatedBacktrack = ErrorMessage.Backtrack(new Position(1, 2),
                                                       ErrorMessageList.Empty
                                                           .With(ErrorMessage.Expected("E"))
                                                           .With(ErrorMessage.Expected("F")));

            ErrorMessageList.Empty
                .With(deepBacktrack)
                .ToString().ShouldEqual("[(3, 4): A or B expected]");

            ErrorMessageList.Empty
                .With(shallowBacktrack)
                .ToString().ShouldEqual("[(2, 3): C or D expected [(3, 4): A or B expected]]");

            ErrorMessageList.Empty
                .With(ErrorMessage.Expected("G"))
                .With(ErrorMessage.Expected("H"))
                .With(shallowBacktrack)
                .With(unrelatedBacktrack)
                .ToString().ShouldEqual("G or H expected [(1, 2): E or F expected] [(2, 3): C or D expected [(3, 4): A or B expected]]");
        }

        [Test]
        public void CanMergeTwoLists()
        {
            var first = ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"))
                .With(ErrorMessage.Unknown())
                .With(ErrorMessage.Expected("C"));

            var second = ErrorMessageList.Empty
                .With(ErrorMessage.Expected("D"))
                .With(ErrorMessage.Expected("B"))
                .With(ErrorMessage.Unknown())
                .With(ErrorMessage.Expected("E"));

            first.Merge(second)
                .ToString().ShouldEqual("A, B, C, D or E expected");
        }

        [Test]
        public void OmitsUnknownErrorsWhenAdditionalErrorsExist()
        {
            ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"))
                .With(ErrorMessage.Unknown())
                .With(ErrorMessage.Expected("C"))
                .ToString().ShouldEqual("A, B or C expected");
        }
    }
}