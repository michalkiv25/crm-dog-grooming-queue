using DogQueue.WebApi.Models;
using DogQueue.WebApi.Models.Auth;

namespace DogQueue.WebApi.Interfaces.Managers;

public interface IAuthManager
{
    ServiceResult<object?> Register(User user);
    ServiceResult<LoginResponse> Login(LoginRequest login);
}
