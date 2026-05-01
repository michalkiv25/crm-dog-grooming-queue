using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DogQueueApi.Data;

/// <summary>
/// Enables <c>dotnet ef migrations</c> / <c>dotnet ef database update</c> without bootstrapping the web host.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var conn =
            Environment.GetEnvironmentVariable("DOGQUEUE_DESIGN_CONNECTION")
            ?? "Data Source=dogqueue.db";

        optionsBuilder.UseSqlite(conn);
        return new AppDbContext(optionsBuilder.Options);
    }
}
