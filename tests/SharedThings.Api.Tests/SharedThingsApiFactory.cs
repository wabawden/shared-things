using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedThings.Api.Data;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Respawn.Graph;
using SharedThings.Api.Interfaces;
using Xunit;

namespace SharedThings.Api.Tests;

public sealed class SharedThingsApiFactory :
    WebApplicationFactory<Program>,
    IAsyncLifetime
{
    private Respawner _respawner = null!;
    
    public FakeItemImageStorage ImageStorage =
        new();
    
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
                            _postgres.GetConnectionString(),
                        ["ItemImages:BucketName"] =
                            "shared-things-tests",
                        ["ItemImages:Region"] =
                            "eu-west-2",
                        ["ItemImages:PublicBaseUrl"] =
                            "https://images.test"
                    });
            });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IItemImageStorage>();

            services.AddSingleton<IItemImageStorage>(
                ImageStorage);
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        await using (var scope =
                     Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<SharedThingsDbContext>();

            await db.Database.MigrateAsync();
            await TestDataSeeder.SeedAsync(db);
        }

        await using var connection =
            new NpgsqlConnection(
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
                    new Table("__EFMigrationsHistory"),
                ],
            });
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null)
        {
            throw new InvalidOperationException(
                "The test database has not been initialised.");
        }

        await using var connection =
            new NpgsqlConnection(
                _postgres.GetConnectionString());

        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);

        await using var scope =
            Services.CreateAsyncScope();

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
