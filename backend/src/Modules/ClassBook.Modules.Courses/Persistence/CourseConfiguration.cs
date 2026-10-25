using ClassBook.Modules.Courses.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassBook.Modules.Courses.Persistence;

internal sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Course");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(Course.NameMaxLength).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(Course.DescriptionMaxLength);
        builder.Property(c => c.MonthlyFee).HasPrecision(18, 2);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Version).IsRowVersion();

        builder.HasIndex(c => new { c.TenantId, c.Name });
    }
}
