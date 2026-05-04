namespace DogQueue.WebApi.Infrastructure;

public static class UsernameNormalizer
{
    public static string Canonical(string? username) =>
        (username ?? "").Trim().ToLowerInvariant();
}
