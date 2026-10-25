using Microsoft.Extensions.Options;

namespace ClassBook.Shared.Tenancy;

// Now: returns the single configured tenant. Later: resolve from host name or user claim.
internal sealed class SingleTenantContext(IOptions<TenancyOptions> options) : ITenantContext
{
    public Guid TenantId => options.Value.DefaultTenantId;
}
