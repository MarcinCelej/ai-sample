using ClassBook.Shared.Persistence;
using ClassBook.Shared.Tenancy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClassBook.IntegrationTests.Infrastructure;

/// <summary>
/// The real API with three test seams: the test database, header-based authentication and a header-based tenant.
/// </summary>
public sealed class ClassBookApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting($"ConnectionStrings:{PersistenceExtensions.ConnectionStringName}", connectionString);

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, null);

            services.RemoveAll<ITenantContext>();
            services.AddScoped<ITenantContext, HeaderTenantContext>();
        });
    }
}
