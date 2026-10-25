using ClassBook.Modules.Courses.Domain;
using ClassBook.Shared.Persistence;
using ClassBook.Shared.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace ClassBook.Modules.Courses.Persistence;

internal sealed class CoursesDbContext(DbContextOptions<CoursesDbContext> options, ITenantContext tenant)
    : ModuleDbContext(options, tenant), IModuleDbContext
{
    public static string Schema => "courses";

    public DbSet<Course> Courses => Set<Course>();

    protected override string DefaultSchema => Schema;
}
