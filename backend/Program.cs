using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DogQueueApi.Configuration;
using DogQueueApi.Data;
using DogQueueApi.Data.Repositories;
using DogQueueApi.Interfaces.Managers;
using DogQueueApi.Interfaces.Repositories;
using DogQueueApi.Interfaces.Providers;
using DogQueueApi.Managers;
using DogQueueApi.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DogQueueApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Render / Fly.io / Railway set PORT; the process must listen on 0.0.0.0, not localhost-only.
var portEnv = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(portEnv))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{portEnv.Trim()}");
}

builder.Services.AddControllers();

// Bind Jwt from configuration, then fix SecretKey: empty Jwt__SecretKey in the environment overrides JSON in
// IConfiguration and breaks Render — ResolveJwtSecret reads appsettings.Production.json from disk if needed.
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
JwtSettings.ResolveJwtSecret(jwtSettings, builder.Environment.ContentRootPath);
builder.Services.AddSingleton<IOptions<JwtSettings>>(_ => Options.Create(jwtSettings));

// LocalDB is Windows-only; using it on Linux (e.g. Render with wrong ASPNETCORE_ENVIRONMENT) often crashes the process (exit 139).
var connProbe = builder.Configuration.GetConnectionString("DefaultConnection");
if (OperatingSystem.IsLinux() &&
    !string.IsNullOrWhiteSpace(connProbe) &&
    connProbe.Contains("localdb", StringComparison.OrdinalIgnoreCase) &&
    !connProbe.Trim().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "Linux cannot use SQL Server LocalDB. Set ASPNETCORE_ENVIRONMENT=Production (Dockerfile does this) " +
        "or set ConnectionStrings__DefaultConnection to SQLite (Data Source=...) or a real SQL Server host.");
}

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(defaultConnection) &&
    defaultConnection.Trim().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
{
    var pathPart = defaultConnection.Trim()["Data Source=".Length..].Trim();
    if (!Path.IsPathRooted(pathPart))
        pathPart = Path.Combine(builder.Environment.ContentRootPath, pathPart);
    defaultConnection = "Data Source=" + pathPart;
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (!string.IsNullOrWhiteSpace(defaultConnection) &&
        defaultConnection.Trim().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(defaultConnection);
    }
    else
    {
        options.UseSqlServer(defaultConnection);
    }
});
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAuthManager, AuthManager>();
builder.Services.AddScoped<IAppointmentsManager, AppointmentsManager>();
builder.Services.AddSingleton<ICurrentUserProvider, CurrentUserProvider>();
builder.Services.AddSingleton<ITokenProvider, JwtTokenProvider>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Migrations target SQL Server; SQLite uses SqliteSchemaFixer instead of Migrate().
    if (!db.Database.IsSqlite())
        db.Database.Migrate();

    SqliteSchemaFixer.Apply(db);
    SqlServerRoutineInstaller.Apply(db);
}

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
