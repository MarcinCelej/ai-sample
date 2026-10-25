namespace ClassBook.Shared.Tenancy;

/// <summary>
/// An entity that belongs to one tenant. The tenant query filter and <see cref="Persistence.TenantInterceptor"/>
/// handle <see cref="TenantId"/>; application code never sets it.
/// </summary>
public interface ITenantOwned
{
    Guid TenantId { get; }
}
