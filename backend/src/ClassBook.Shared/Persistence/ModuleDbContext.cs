using ClassBook.Shared.Domain;
using ClassBook.Shared.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace ClassBook.Shared.Persistence;

/// <summary>
/// Base class of every module <c>DbContext</c> (except Identity): sets the module schema, applies the
/// entity configurations of the module assembly and the tenant filter on every <see cref="ITenantOwned"/> entity.
/// </summary>
public abstract class ModuleDbContext(DbContextOptions options, ITenantContext tenant) : DbContext(options)
{
    /// <summary>Name of the EF Core named query filter that scopes data to the current tenant.</summary>
    public const string TenantFilter = "Tenant";

    private readonly ITenantContext _tenant = tenant;

    protected abstract string DefaultSchema { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        foreach (var entity in modelBuilder.Model.GetEntityTypes().Where(e => typeof(ITenantOwned).IsAssignableFrom(e.ClrType)))
        {
            modelBuilder.Entity(entity.ClrType).SetTenantFilter(TenantFilter, () => _tenant.TenantId);
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Money>().HaveConversion<MoneyConverter>().HavePrecision(18, 2);
    }
}
