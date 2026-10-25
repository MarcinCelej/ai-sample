using ClassBook.Modules.Courses.Domain;
using static ClassBook.Modules.Courses.CreateCourse.CreateCourse;

namespace ClassBook.UnitTests.Courses;

public class CreateCourseValidatorTests
{
    private static readonly Request Valid = new("Chess", "Beginners", 12, 150.50m);

    // Keyed by the expected error code; request types are internal, so theories pass only the key.
    private static readonly Dictionary<string, Request> InvalidRequests = new()
    {
        ["course.name.required"] = Valid with { Name = "" },
        ["course.name.tooLong"] = Valid with { Name = new string('a', Course.NameMaxLength + 1) },
        ["course.description.tooLong"] = Valid with { Description = new string('a', Course.DescriptionMaxLength + 1) },
        ["course.capacity.min"] = Valid with { Capacity = 0 },
        ["course.monthlyFee.min"] = Valid with { MonthlyFee = -1m },
        ["course.monthlyFee.precision"] = Valid with { MonthlyFee = 10.005m },
    };

    private readonly Validator _validator = new();

    public static TheoryData<string> InvalidRequestCodes() => [.. InvalidRequests.Keys];

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        _validator.Validate(Valid).IsValid.ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidRequestCodes))]
    public void Validate_InvalidRequest_ReturnsErrorCode(string expectedCode)
    {
        _validator.Validate(InvalidRequests[expectedCode]).Errors.Select(e => e.ErrorCode).ShouldBe([expectedCode]);
    }
}
