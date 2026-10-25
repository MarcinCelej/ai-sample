using ClassBook.Shared.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ClassBook.IntegrationTests.Infrastructure;

/// <summary>
/// The tenant from the <c>X-Test-Tenant</c> header, or the configured default tenant
/// (also outside requests, e.g. when a test seeds data through a <c>DbContext</c>).
/// </summary>
public sealed class HeaderTenantContext(IHttpContextAccessor httpContextAccessor, IOptions<TenancyOptions> options) : ITenantContext
{
    public const string TenantHeader = "X-Test-Tenant";

    public Guid TenantId =>
        Guid.TryParse(httpContextAccessor.HttpContext?.Request.Headers[TenantHeader], out var tenantId)
            ? tenantId
            : options.Value.DefaultTenantId;
}
