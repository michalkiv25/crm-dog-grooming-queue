namespace DogQueueApi.Infrastructure;

/// <summary>
/// One canonical form for usernames everywhere (JWT, DB, loyalty counts) so SQLite/SQL Server
/// equality matches reliably and we avoid fragile translated LINQ (trim/lower on columns).
/// </summary>
public static class UsernameNormalizer
{
    public static string Canonical(string? username) =>
        (username ?? "").Trim().ToLowerInvariant();
}
