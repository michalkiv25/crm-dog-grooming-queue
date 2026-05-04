using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DogQueueApi.Tests;

/// <summary>
/// Test host with deterministic JWT secret and isolated SQLite file so tests do not touch dev DB.
/// <see cref="UseSetting"/> overrides appsettings.json (e.g. LocalDB) so integration tests run on Mac/Linux.
/// </summary>
public class DogQueueApiApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _testDbPath =
        Path.Combine(Path.GetTempPath(), $"dogqueue-api-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Must win over appsettings.json LocalDB — InMemoryCollection alone can lose merge order with minimal hosting.
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_testDbPath}");
        builder.UseSetting("Jwt:SecretKey", "TestJwtSecret_KeyMustBeAtLeast32CharsLong!");
        builder.UseSetting("Jwt:Issuer", "DogQueueApi");
        builder.UseSetting("Jwt:Audience", "DogQueueApi");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try
        {
            if (File.Exists(_testDbPath))
                File.Delete(_testDbPath);
        }
        catch
        {
            // ignore temp cleanup failures
        }
    }
}
