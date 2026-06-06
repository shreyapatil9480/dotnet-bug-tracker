using BugTracker.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BugTracker.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that configures in-memory SQLite for integration tests.
/// Each test class gets a fresh database instance.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            services.RemoveAll<DbContextOptions<BugTrackerDbContext>>();
            services.RemoveAll<BugTrackerDbContext>();

            // Create and open a connection to in-memory SQLite
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // Add DbContext with in-memory SQLite
            services.AddDbContext<BugTrackerDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Build service provider and create database
            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BugTrackerDbContext>();
            dbContext.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}

/// <summary>
/// WebApplicationFactory configured for the Test environment (BDD reset endpoint).
/// </summary>
public class TestEnvironmentWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<BugTrackerDbContext>>();
            services.RemoveAll<BugTrackerDbContext>();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<BugTrackerDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });
        });

        builder.UseEnvironment("Test");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}
