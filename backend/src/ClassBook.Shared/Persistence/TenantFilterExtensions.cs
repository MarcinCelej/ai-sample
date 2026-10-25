using System.Linq.Expressions;
using ClassBook.Shared.Tenancy;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClassBook.Shared.Persistence;

public static class TenantFilterExtensions
{
    /// <summary>
    /// Adds the named query filter <c>e => e.TenantId == tenantId()</c>. <paramref name="tenantId"/> must read a
    /// member of the <c>DbContext</c> instance so EF Core evaluates it per context, not once per model.
    /// </summary>
    public static EntityTypeBuilder SetTenantFilter(this EntityTypeBuilder entity, string filterKey, Expression<Func<Guid>> tenantId)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(tenantId);

        var e = Expression.Parameter(entity.Metadata.ClrType, "e");
        var body = Expression.Equal(Expression.Property(e, nameof(ITenantOwned.TenantId)), tenantId.Body);
        return entity.HasQueryFilter(filterKey, Expression.Lambda(body, e));
    }
}
