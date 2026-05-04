using System.Security.Claims;

namespace DogQueue.WebApi.Interfaces.Providers;

public interface ICurrentUserProvider
{
    string? GetUsername(ClaimsPrincipal user);
}
