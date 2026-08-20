using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedThings.Api.Data;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Xunit;

namespace SharedThings.Api.Tests;

public sealed class SharedThingsApiFactory :
    WebApplicationFactory<Program>,
    IAsyncLifetime
{
    private Respawner _respawner = null!;
    
    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase("shared_things_tests")
            .WithUsername("shared_things_tests")
            .WithPassword("test-password")
            .Build();

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:SharedThings"] =
                            _postgres.GetConnectionString()
                    });
            });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<SharedThingsDbContext>();

            await db.Database.MigrateAsync();
            await TestDataSeeder.SeedAsync(db);
        }

        await using var connection = new NpgsqlConnection(
            _postgres.GetConnectionString());

        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"],
                TablesToIgnore =
                [
                    new Table("__EFMigrationsHistory")
                ]
            });
    }
    
    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(
            _postgres.GetConnectionString());

        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);

        using var scope = Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<SharedThingsDbContext>();

        await TestDataSeeder.SeedAsync(db);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}