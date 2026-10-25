using ClassBook.Shared.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClassBook.Shared.Persistence;

/// <summary>Sets <see cref="ITenantOwned.TenantId"/> on added entities and prevents changing it later.</summary>
public sealed class TenantInterceptor(ITenantContext tenant) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return ValueTask.FromResult(result);
    }

    internal void Apply(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<ITenantOwned>())
        {
            var tenantId = entry.Property(e => e.TenantId);
            if (entry.State == EntityState.Added)
            {
                tenantId.CurrentValue = tenant.TenantId;
            }
            else if (entry.State == EntityState.Modified && tenantId.IsModified)
            {
                throw new InvalidOperationException($"TenantId of {entry.Metadata.DisplayName()} must not change.");
            }
        }
    }
}
