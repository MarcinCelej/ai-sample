namespace ClassBook.Shared.Security;

/// <summary>Named authorization policies used by endpoints.</summary>
public static class Policies
{
    public const string ManageCourses = nameof(ManageCourses);      // Administrator
    public const string ManagePayments = nameof(ManagePayments);    // Administrator
    public const string ViewOwnClasses = nameof(ViewOwnClasses);    // Instructor, Administrator
}
