using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DogQueueApi.Configuration;
using DogQueueApi.Data;
using DogQueueApi.Interfaces.Managers;
using DogQueueApi.Interfaces.Providers;
using DogQueueApi.Managers;
using DogQueueApi.Providers;
using Microsoft.EntityFrameworkCore;
using DogQueueApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Render / Fly.io / Railway set PORT; the process must listen on 0.0.0.0, not localhost-only.
var portEnv = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(portEnv))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{portEnv.Trim()}");
}

builder.Services.AddControllers();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.PostConfigure<JwtSettings>(JwtSettings.OverwriteSecretFromEnvironment);

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
JwtSettings.OverwriteSecretFromEnvironment(jwtSettings);
if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:SecretKey is missing or shorter than 32 characters. " +
        "Local: use appsettings.Development.json. Render: Dashboard → Web Service → Environment → add " +
        "Key Jwt__SecretKey (two underscores) with a random value of at least 32 characters, " +
        "or Key JWT_SECRET with the same. Then redeploy.");
}

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
