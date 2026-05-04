using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DogQueue.WebApi.Data;

/// <summary>
/// Design-time factory for <c>dotnet ef migrations</c>. Uses SQL Server only.
/// Override with <c>DOGQUEUE_DESIGN_CONNECTION</c> if your design-time DB differs from LocalDB.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var conn =
            Environment.GetEnvironmentVariable("DOGQUEUE_DESIGN_CONNECTION")
            ?? "Server=(localdb)\\mssqllocaldb;Database=DogQueueApi_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(conn);
        return new AppDbContext(optionsBuilder.Options);
    }
}
