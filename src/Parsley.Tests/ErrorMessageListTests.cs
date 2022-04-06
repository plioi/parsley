namespace Parsley.Tests
{
    using Shouldly;
    using ErrorMessage = Parsley.ErrorMessage;

    public class ErrorMessageListTests
    {
        public void ShouldProvideSharedEmptyInstance()
        {
            ErrorMessageList.Empty.ShouldBeSameAs(ErrorMessageList.Empty);
        }

        public void CanBeEmpty()
        {
            ErrorMessageList.Empty.ToString().ShouldBe("");
        }

        public void CreatesNewCollectionWhenAddingItems()
        {
            ErrorMessageList list = ErrorMessageList.Empty.With(ErrorMessage.Expected("expectation"));

            list.ToString().ShouldBe("expectation expected");
            list.ShouldNotBeSameAs(ErrorMessageList.Empty);
        }

        public void CanIncludeUnknownErrors()
        {
            ErrorMessageList.Empty
                .With(ErrorMessage.Unknown())
                .ToString().ShouldBe("Parse error.");
        }

        public void CanIncludeMultipleExpectations()
        {
            ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"))
                .ToString().ShouldBe("A or B expected");

            ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"))
                .With(ErrorMessage.Expected("C"))
                .ToString().ShouldBe("A, B or C expected");

            ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"))
                .With(ErrorMessage.Expected("C"))
                .With(ErrorMessage.Expected("D"))
                .ToString().ShouldBe("A, B, C or D expected");
        }

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
                .ToString().ShouldBe("A, B or C expected");
        }

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
                .ToString().ShouldBe("[(3, 4): A or B expected]");

            ErrorMessageList.Empty
                .With(shallowBacktrack)
                .ToString().ShouldBe("[(2, 3): C or D expected [(3, 4): A or B expected]]");

            ErrorMessageList.Empty
                .With(ErrorMessage.Expected("G"))
                .With(ErrorMessage.Expected("H"))
                .With(shallowBacktrack)
                .With(unrelatedBacktrack)
                .ToString().ShouldBe("G or H expected [(1, 2): E or F expected] [(2, 3): C or D expected [(3, 4): A or B expected]]");
        }

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
                .ToString().ShouldBe("A, B, C, D or E expected");
        }

        public void OmitsUnknownErrorsWhenAdditionalErrorsExist()
        {
            ErrorMessageList.Empty
                .With(ErrorMessage.Expected("A"))
                .With(ErrorMessage.Expected("B"))
                .With(ErrorMessage.Unknown())
                .With(ErrorMessage.Expected("C"))
                .ToString().ShouldBe("A, B or C expected");
        }
    }
}