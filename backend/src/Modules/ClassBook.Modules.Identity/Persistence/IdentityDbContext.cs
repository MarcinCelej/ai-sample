using ClassBook.Modules.Identity.Domain;
using ClassBook.Shared.Persistence;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClassBook.Modules.Identity.Persistence;

/// <summary>
/// ASP.NET Core Identity stores in the <c>identity</c> schema. Unlike other module contexts it derives from the
/// Identity context, not <see cref="ModuleDbContext"/>: Identity tables are not tenant-owned.
/// </summary>
internal sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : IdentityDbContext<AppUser, AppRole, Guid>(options), IModuleDbContext
{
    public static string Schema => "identity";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(Schema);
    }
}
