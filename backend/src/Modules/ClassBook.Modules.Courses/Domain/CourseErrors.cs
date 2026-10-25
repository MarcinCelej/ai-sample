using ClassBook.Shared.Errors;

namespace ClassBook.Modules.Courses.Domain;

internal static class CourseErrors
{
    public static readonly Error AlreadyArchived = new("course.alreadyArchived", "Course is already archived.", ErrorType.Conflict);
}
