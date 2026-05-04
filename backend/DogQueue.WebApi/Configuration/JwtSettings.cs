using System.Text.Json;

namespace DogQueue.WebApi.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public const string RenderEmbeddedFallbackSecret =
        "DogQueueEmbeddedJwtSigningKey_RenderFallback_MinLength32_ReplaceForProd__";

    public string SecretKey { get; set; } = "";

    public string Issuer { get; set; } = "DogQueueApi";

    public string Audience { get; set; } = "DogQueueApi";

    public int ExpiryHours { get; set; } = 2;

    public static void OverwriteSecretFromEnvironment(JwtSettings settings)
    {
        var v = Environment.GetEnvironmentVariable("Jwt__SecretKey")?.Trim()
            ?? Environment.GetEnvironmentVariable("JWT_SECRET")?.Trim();
        if (!string.IsNullOrWhiteSpace(v))
            settings.SecretKey = v;
    }

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
