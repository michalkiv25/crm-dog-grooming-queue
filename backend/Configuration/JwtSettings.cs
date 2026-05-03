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
}
