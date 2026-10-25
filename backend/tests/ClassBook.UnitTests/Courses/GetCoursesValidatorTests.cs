using ClassBook.Shared.Paging;
using static ClassBook.Modules.Courses.GetCourses.GetCourses;

namespace ClassBook.UnitTests.Courses;

public class GetCoursesValidatorTests
{
    private readonly Validator _validator = new();

    [Theory]
    [InlineData(1, 1)]
    [InlineData(5, Paging.MaxPageSize)]
    public void Validate_ValidPaging_HasNoErrors(int page, int pageSize)
    {
        _validator.Validate(new Request(page, pageSize)).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0, 20, Paging.PageInvalidCode)]
    [InlineData(1, 0, Paging.PageSizeInvalidCode)]
    [InlineData(1, Paging.MaxPageSize + 1, Paging.PageSizeInvalidCode)]
    public void Validate_InvalidPaging_ReturnsErrorCode(int page, int pageSize, string expectedCode)
    {
        _validator.Validate(new Request(page, pageSize)).Errors.Select(e => e.ErrorCode).ShouldBe([expectedCode]);
    }

    [Fact]
    public void Request_DefaultsToFirstPageOfDefaultSize()
    {
        new Request().ShouldBe(new Request(1, Paging.DefaultPageSize));
    }
}
