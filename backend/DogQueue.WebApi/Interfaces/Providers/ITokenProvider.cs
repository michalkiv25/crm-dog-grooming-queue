using DogQueue.WebApi.Models;

namespace DogQueue.WebApi.Interfaces.Providers;

public interface ITokenProvider
{
    string CreateToken(User user);
}
