using DogQueueApi.Models;

namespace DogQueueApi.Interfaces.Providers
{
    public interface ITokenProvider
    {
        string CreateToken(User user);
    }
}
