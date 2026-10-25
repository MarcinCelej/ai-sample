using ClassBook.Shared.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClassBook.Shared.Persistence;

/// <summary>Fills the <see cref="IAuditable"/> fields from <see cref="TimeProvider"/> and the current user.</summary>
public sealed class AuditInterceptor(TimeProvider timeProvider, ICurrentUser currentUser) : SaveChangesInterceptor
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

        var now = timeProvider.GetUtcNow();
        var userId = currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(e => e.CreatedAt).CurrentValue = now;
                entry.Property(e => e.CreatedBy).CurrentValue = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.UpdatedAt).CurrentValue = now;
                entry.Property(e => e.UpdatedBy).CurrentValue = userId;
            }
        }
    }
}
