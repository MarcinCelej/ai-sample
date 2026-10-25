using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClassBook.Shared.Persistence;

public static class PersistenceExtensions
{
    public const string ConnectionStringName = "ClassBook";

    /// <summary>
    /// Registers a module <c>DbContext</c>: shared connection string, migrations history table in the module schema,
    /// tenant and audit interceptors. The connection string is read lazily, when the context is first created.
    /// </summary>
    public static IServiceCollection AddModuleDbContext<TContext>(this IServiceCollection services)
        where TContext : DbContext, IModuleDbContext
    {
        services.AddDbContext<TContext>((sp, options) =>
        {
            var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName);
            if (connectionString is null && !EF.IsDesignTime)
            {
                throw new InvalidOperationException(
                    $"Connection string '{ConnectionStringName}' is not configured. Set it with 'dotnet user-secrets'.");
            }

            // 'dotnet ef migrations add' needs no database, so design time works without a connection string.
            options.UseSqlServer(connectionString, sql => sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, TContext.Schema));
            options.AddInterceptors(sp.GetRequiredService<TenantInterceptor>(), sp.GetRequiredService<AuditInterceptor>());
        });

        services.AddSingleton(new ModuleDbContextRegistration(typeof(TContext)));
        return services;
    }

    /// <summary>Applies pending migrations of every registered module <c>DbContext</c>.</summary>
    public static async Task MigrateModuleDatabasesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        foreach (var registration in scope.ServiceProvider.GetServices<ModuleDbContextRegistration>())
        {
            var context = (DbContext)scope.ServiceProvider.GetRequiredService(registration.ContextType);
            await context.Database.MigrateAsync(cancellationToken);
        }
    }

    /// <inheritdoc cref="MigrateModuleDatabasesAsync(IServiceProvider, CancellationToken)"/>
    public static Task MigrateModuleDatabasesAsync(this WebApplication app) => app.Services.MigrateModuleDatabasesAsync(app.Lifetime.ApplicationStopping);
}
