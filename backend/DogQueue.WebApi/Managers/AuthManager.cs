using DogQueue.WebApi.Infrastructure;
using DogQueue.WebApi.Interfaces.Managers;
using DogQueue.WebApi.Interfaces.Providers;
using DogQueue.WebApi.Interfaces.Repositories;
using DogQueue.WebApi.Models;
using DogQueue.WebApi.Models.Auth;
using DogQueue.WebApi.Validators;

namespace DogQueue.WebApi.Managers;

public class AuthManager : IAuthManager
{
    private readonly IUserRepository _users;
    private readonly ITokenProvider _tokenProvider;

    public AuthManager(IUserRepository users, ITokenProvider tokenProvider)
    {
        _users = users;
        _tokenProvider = tokenProvider;
    }

    public ServiceResult<object?> Register(User user)
    {
        user.Username = UsernameNormalizer.Canonical(user.Username);

        var (isValid, errors) = UserValidator.ValidateRegister(user);
        if (!isValid)
        {
            return ServiceResult<object?>.BadRequest("Validation failed", errors);
        }

        if (_users.UsernameExists(user.Username))
        {
            return ServiceResult<object?>.BadRequest("Username already exists");
        }

        _users.Add(user);
        _users.SaveChanges();

        return ServiceResult<object?>.Ok(new { message = "User registered successfully" });
    }

    public ServiceResult<LoginResponse> Login(LoginRequest login)
    {
        var (isValid, errors) = UserValidator.ValidateLogin(login);
        if (!isValid)
        {
            return ServiceResult<LoginResponse>.BadRequest("Validation failed", errors);
        }

        var loginName = UsernameNormalizer.Canonical(login.Username);
        var user = _users.FindByUsernameAndPassword(loginName, login.Password);

        if (user == null)
        {
            return ServiceResult<LoginResponse>.Unauthorized("Invalid credentials");
        }

        var jwt = _tokenProvider.CreateToken(user);

        var response = new LoginResponse
        {
            Token = jwt,
            Username = user.Username,
            Fullname = user.FullName
        };

        return ServiceResult<LoginResponse>.Ok(response);
    }
}
