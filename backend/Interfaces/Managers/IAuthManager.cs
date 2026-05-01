using DogQueueApi.Models;
using DogQueueApi.Models.Auth;
using DogQueueApi.Services;

namespace DogQueueApi.Interfaces.Managers
{
    public interface IAuthManager
    {
        ServiceResult<object?> Register(User user);
        ServiceResult<LoginResponse> Login(LoginRequest login);
    }
}
