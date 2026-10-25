using ClassBook.Shared.Tenancy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClassBook.IntegrationTests.Infrastructure;

/// <summary>Base class: a clean database per test and helpers for clients and direct database access.</summary>
public abstract class IntegrationTest(ApiFixture api) : IAsyncLifetime
{
    private const string AntiforgeryCookie = "XSRF-TOKEN";
    private const string AntiforgeryHeader = "X-XSRF-TOKEN";

    protected Guid DefaultTenantId =>
        api.Factory.Services.GetRequiredService<IOptions<TenancyOptions>>().Value.DefaultTenantId;

    public virtual async ValueTask InitializeAsync() => await api.ResetDatabaseAsync();

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    /// <summary>An HTTPS client (the CSRF cookie is <c>Secure</c>) for the given user and tenant; anonymous when no user.</summary>
    protected HttpClient CreateClient(TestUser? user = null, Guid? tenantId = null)
    {
        var client = api.Factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        if (user is not null)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, user.Id.ToString());
            client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Join(',', user.Roles));
        }

        if (tenantId is { } tenant)
        {
            client.DefaultRequestHeaders.Add(HeaderTenantContext.TenantHeader, tenant.ToString());
        }

        return client;
    }

    /// <summary>A client that, like the SPA, has fetched the CSRF token and sends it on unsafe requests.</summary>
    protected async Task<HttpClient> CreateClientWithAntiforgeryAsync(TestUser? user = null, Guid? tenantId = null)
    {
        var client = CreateClient(user, tenantId);
        using var response = await client.GetAsync(new Uri("/api/courses", UriKind.Relative));
        var token = response.Headers.GetValues("Set-Cookie")
            .Select(cookie => cookie.Split(';')[0])
            .Single(cookie => cookie.StartsWith(AntiforgeryCookie + "=", StringComparison.Ordinal))[(AntiforgeryCookie.Length + 1)..];
        client.DefaultRequestHeaders.Add(AntiforgeryHeader, token);
        return client;
    }

    /// <summary>Runs <paramref name="action"/> with a service scope, e.g. to seed or inspect data via a module context.</summary>
    protected async Task WithScopeAsync(Func<IServiceProvider, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        await using var scope = api.Factory.Services.CreateAsyncScope();
        await action(scope.ServiceProvider);
    }
}
