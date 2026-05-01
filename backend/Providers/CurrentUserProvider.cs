using DogQueueApi.Interfaces.Providers;
using System.Security.Claims;

namespace DogQueueApi.Providers
{
    public class CurrentUserProvider : ICurrentUserProvider
    {
        public string? GetUsername(ClaimsPrincipal user)
        {
            return user.Identity?.Name
                ?? user.FindFirstValue(ClaimTypes.Name)
                ?? user.FindFirstValue("username")
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
