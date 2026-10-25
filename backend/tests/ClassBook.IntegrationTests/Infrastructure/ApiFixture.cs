using ClassBook.Shared.Persistence;
using Microsoft.Data.SqlClient;
using Respawn;
using Testcontainers.MsSql;

namespace ClassBook.IntegrationTests.Infrastructure;

/// <summary>
/// Starts SQL Server in a container, applies the migrations of all module contexts and hosts the API in memory.
/// Requires Docker.
/// </summary>
public sealed class ApiFixture : IAsyncLifetime
{
    private static readonly string[] ModuleSchemas = ["identity", "courses", "families", "enrollments", "payments"];

    private readonly MsSqlContainer _sql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    private Respawner? _respawner;
    private string _connectionString = "";

    public ClassBookApiFactory Factory { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _sql.StartAsync();
        _connectionString = new SqlConnectionStringBuilder(_sql.GetConnectionString()) { InitialCatalog = "ClassBook" }.ConnectionString;

        Factory = new ClassBookApiFactory(_connectionString);
        await Factory.Services.MigrateModuleDatabasesAsync();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = ModuleSchemas,
            TablesToIgnore = [new Respawn.Graph.Table("__EFMigrationsHistory")],
        });
    }

    /// <summary>Deletes all data except the migrations history.</summary>
    public async Task ResetDatabaseAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await _respawner!.ResetAsync(connection);
    }

    public async ValueTask DisposeAsync()
    {
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        await _sql.DisposeAsync();
    }
}
