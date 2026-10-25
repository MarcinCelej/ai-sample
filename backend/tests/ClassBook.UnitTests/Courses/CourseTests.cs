using ClassBook.Modules.Courses.Domain;
using ClassBook.Shared.Domain;

namespace ClassBook.UnitTests.Courses;

public class CourseTests
{
    [Fact]
    public void Create_ReturnsActiveCourseWithTrimmedText()
    {
        var course = Course.Create("  Chess  ", "  Beginners  ", 12, Money.Pln(150m));

        course.Name.ShouldBe("Chess");
        course.Description.ShouldBe("Beginners");
        course.Capacity.ShouldBe(12);
        course.MonthlyFee.ShouldBe(Money.Pln(150m));
        course.Status.ShouldBe(CourseStatus.Active);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_StoresBlankDescriptionAsNull(string? description)
    {
        Course.Create("Chess", description, 12, Money.Zero).Description.ShouldBeNull();
    }

    [Fact]
    public void Create_WithBlankName_Throws()
    {
        Should.Throw<ArgumentException>(() => Course.Create(" ", null, 1, Money.Zero));
    }

    [Fact]
    public void Create_WithTooLongName_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Course.Create(new string('a', Course.NameMaxLength + 1), null, 1, Money.Zero));
    }

    [Fact]
    public void Create_WithTooLongDescription_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => Course.Create("Chess", new string('a', Course.DescriptionMaxLength + 1), 1, Money.Zero));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithCapacityBelowOne_Throws(int capacity)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Course.Create("Chess", null, capacity, Money.Zero));
    }

    [Fact]
    public void Create_WithNegativeFee_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Course.Create("Chess", null, 1, Money.Pln(-0.01m)));
    }

    [Fact]
    public void Archive_ActiveCourse_ArchivesIt()
    {
        var course = Course.Create("Chess", null, 12, Money.Zero);

        var result = course.Archive();

        result.IsSuccess.ShouldBeTrue();
        course.Status.ShouldBe(CourseStatus.Archived);
    }

    [Fact]
    public void Archive_ArchivedCourse_ReturnsAlreadyArchived()
    {
        var course = Course.Create("Chess", null, 12, Money.Zero);
        course.Archive();

        var result = course.Archive();

        result.Error.ShouldBe(CourseErrors.AlreadyArchived);
    }
}
