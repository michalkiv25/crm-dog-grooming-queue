using DogQueueApi.Infrastructure;
using DogQueueApi.Interfaces.Providers;
using System.Security.Claims;

namespace DogQueueApi.Providers
{
    public class CurrentUserProvider : ICurrentUserProvider
    {
        public string? GetUsername(ClaimsPrincipal user)
        {
            var name = user.Identity?.Name
                ?? user.FindFirstValue(ClaimTypes.Name)
                ?? user.FindFirstValue("username")
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrWhiteSpace(name) ? null : UsernameNormalizer.Canonical(name);
        }
    }
}
