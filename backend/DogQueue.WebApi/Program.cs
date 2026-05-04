using DogQueue.WebApi.Configuration;
using DogQueue.WebApi.Data;
using DogQueue.WebApi.Data.Repositories;
using DogQueue.WebApi.Infrastructure;
using DogQueue.WebApi.Interfaces.Managers;
using DogQueue.WebApi.Interfaces.Providers;
using DogQueue.WebApi.Interfaces.Repositories;
using DogQueue.WebApi.Managers;
using DogQueue.WebApi.Providers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var portEnv = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(portEnv))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{portEnv.Trim()}");
}

builder.Services.AddControllers();

var jwtSettings = new JwtSettings();
builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
JwtSettings.ResolveJwtSecret(jwtSettings, builder.Environment.ContentRootPath);
builder.Services.AddSingleton<IOptions<JwtSettings>>(_ => Options.Create(jwtSettings));

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")?.Trim();
if (string.IsNullOrWhiteSpace(defaultConnection))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is required. This API uses Microsoft SQL Server only. " +
        "Set it in appsettings or the ConnectionStrings__DefaultConnection environment variable.");
}

if (OperatingSystem.IsLinux() &&
    defaultConnection.Contains("localdb", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "Linux cannot use SQL Server LocalDB. Point ConnectionStrings__DefaultConnection at a real SQL Server instance " +
        "(e.g. Docker or a cloud host).");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(defaultConnection));

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
    db.Database.Migrate();
    SqlServerRoutineInstaller.Apply(db);
}

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
