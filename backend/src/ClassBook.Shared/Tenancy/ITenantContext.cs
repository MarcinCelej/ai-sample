namespace ClassBook.Shared.Tenancy;

/// <summary>The tenant of the current request.</summary>
public interface ITenantContext
{
    Guid TenantId { get; }
}
