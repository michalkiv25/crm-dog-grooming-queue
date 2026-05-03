using System.Text.Json;

namespace DogQueueApi.Configuration;

/// <summary>
/// JWT signing and validation settings. Bind from configuration section "Jwt"
/// or override with environment variables, e.g. Jwt__SecretKey.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// Last resort if config + file are unusable (e.g. empty <c>Jwt__SecretKey</c> env var overrides JSON).
    /// </summary>
    public const string RenderEmbeddedFallbackSecret =
        "DogQueueEmbeddedJwtSigningKey_RenderFallback_MinLength32_ReplaceForProd__";

    /// <summary>
    /// Symmetric key for HS256; must be long enough for the algorithm (use at least 32 characters).
    /// </summary>
    public string SecretKey { get; set; } = "";

    public string Issuer { get; set; } = "DogQueueApi";

    public string Audience { get; set; } = "DogQueueApi";

    public int ExpiryHours { get; set; } = 2;

    /// <summary>
    /// Render and other hosts sometimes expose secrets only as raw env vars.
    /// Supports <c>Jwt__SecretKey</c> (ASP.NET Core convention) and <c>JWT_SECRET</c> (common in tutorials).
    /// Only applies non-empty values (empty env would otherwise wipe a good appsettings value).
    /// </summary>
    public static void OverwriteSecretFromEnvironment(JwtSettings settings)
    {
        var v = Environment.GetEnvironmentVariable("Jwt__SecretKey")?.Trim()
            ?? Environment.GetEnvironmentVariable("JWT_SECRET")?.Trim();
        if (!string.IsNullOrWhiteSpace(v))
            settings.SecretKey = v;
    }

    /// <summary>
    /// Reads <c>Jwt:SecretKey</c> from <c>appsettings.Production.json</c> on disk, bypassing configuration
    /// merge order (empty <c>Jwt__SecretKey</c> in the environment overrides JSON in <see cref="IConfiguration"/>).
    /// </summary>
    public static string? TryReadSecretFromProductionJson(string contentRoot)
    {
        try
        {
            var path = Path.Combine(contentRoot, "appsettings.Production.json");
            if (!File.Exists(path))
                return null;
            using var stream = File.OpenRead(path);
            using var doc = JsonDocument.Parse(stream);
            if (!doc.RootElement.TryGetProperty("Jwt", out var jwt) ||
                !jwt.TryGetProperty("SecretKey", out var sk))
                return null;
            return sk.GetString();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Ensures <see cref="SecretKey"/> is at least 32 characters: env (if non-empty), then production JSON file, then embedded fallback.
    /// </summary>
    public static void ResolveJwtSecret(JwtSettings settings, string contentRoot)
    {
        OverwriteSecretFromEnvironment(settings);
        if (!string.IsNullOrWhiteSpace(settings.SecretKey) && settings.SecretKey.Length >= 32)
            return;

        var fromFile = TryReadSecretFromProductionJson(contentRoot);
        if (!string.IsNullOrWhiteSpace(fromFile) && fromFile.Length >= 32)
        {
            settings.SecretKey = fromFile;
            return;
        }

        settings.SecretKey = RenderEmbeddedFallbackSecret;
        Console.WriteLine(
            "WARNING: Jwt SecretKey fell back to embedded demo value. " +
            "On Render: delete empty Jwt__SecretKey / JWT_SECRET env entries, or set a real secret (≥32 chars).");
    }
}
