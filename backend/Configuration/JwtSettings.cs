namespace DogQueueApi.Configuration;

/// <summary>
/// JWT signing and validation settings. Bind from configuration section "Jwt"
/// or override with environment variables, e.g. Jwt__SecretKey.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

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
    /// </summary>
    public static void OverwriteSecretFromEnvironment(JwtSettings settings)
    {
        var v = Environment.GetEnvironmentVariable("Jwt__SecretKey")?.Trim()
            ?? Environment.GetEnvironmentVariable("JWT_SECRET")?.Trim();
        if (!string.IsNullOrWhiteSpace(v))
            settings.SecretKey = v;
    }
}
