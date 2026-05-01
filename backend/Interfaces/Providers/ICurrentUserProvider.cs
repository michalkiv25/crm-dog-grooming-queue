using System.Security.Claims;

namespace DogQueueApi.Interfaces.Providers
{
    public interface ICurrentUserProvider
    {
        string? GetUsername(ClaimsPrincipal user);
    }
}
