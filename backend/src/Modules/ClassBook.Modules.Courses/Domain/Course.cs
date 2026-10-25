using ClassBook.Shared.Domain;
using ClassBook.Shared.Errors;
using ClassBook.Shared.Persistence;
using ClassBook.Shared.Tenancy;

namespace ClassBook.Modules.Courses.Domain;

/// <summary>A course in the catalog. Edited concurrently (capacity), so it carries a rowversion.</summary>
internal sealed class Course : ITenantOwned, IAuditable
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    private Course(string name, string? description, int capacity, Money monthlyFee)
    {
        Name = name;
        Description = description;
        Capacity = capacity;
        MonthlyFee = monthlyFee;
        Status = CourseStatus.Active;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public int Capacity { get; private set; }

    public Money MonthlyFee { get; private set; }

    public CourseStatus Status { get; private set; }

    public byte[] Version { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    /// <summary>Creates an active course. Input is validated before; invalid arguments are bugs.</summary>
    public static Course Create(string name, string? description, int capacity, Money monthlyFee)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, NameMaxLength, nameof(name));
        if (description is not null)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(description.Length, DescriptionMaxLength, nameof(description));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        ArgumentOutOfRangeException.ThrowIfNegative(monthlyFee.Amount, nameof(monthlyFee));

        return new Course(name.Trim(), string.IsNullOrWhiteSpace(description) ? null : description.Trim(), capacity, monthlyFee);
    }

    /// <summary>Removes the course from the public catalog. History stays.</summary>
    public Result Archive()
    {
        if (Status == CourseStatus.Archived)
        {
            return CourseErrors.AlreadyArchived;
        }

        Status = CourseStatus.Archived;
        return Result.Success();
    }
}
