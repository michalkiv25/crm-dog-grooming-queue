using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DogQueueApi.Data;
using DogQueueApi.Interfaces.Managers;
using DogQueueApi.Interfaces.Providers;
using DogQueueApi.Managers;
using DogQueueApi.Providers;
using Microsoft.EntityFrameworkCore;
using DogQueueApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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
            ValidIssuer = "DogQueueApi",
            ValidAudience = "DogQueueApi",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("THIS_IS_MY_SUPER_SECRET_KEY_12345"))
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
    SqliteSchemaFixer.Apply(db);
}

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();