namespace ClassBook.Shared.Tenancy;

public sealed class TenancyOptions
{
    public const string SectionName = "Tenancy";

    /// <summary>The single tenant used until multi-tenancy is enabled.</summary>
    public Guid DefaultTenantId { get; set; }
}
