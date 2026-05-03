using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace DogQueueApi.Tests;

/// <summary>
/// Test host with deterministic JWT secret and isolated SQLite file so tests do not touch dev DB.
/// </summary>
public class DogQueueApiApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _testDbPath =
        Path.Combine(Path.GetTempPath(), $"dogqueue-api-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "TestJwtSecret_KeyMustBeAtLeast32CharsLong!",
                ["Jwt:Issuer"] = "DogQueueApi",
                ["Jwt:Audience"] = "DogQueueApi",
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_testDbPath}"
            });
        });
    }
}
