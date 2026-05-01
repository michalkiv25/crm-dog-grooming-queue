using DogQueueApi.Data;
using DogQueueApi.Interfaces.Managers;
using DogQueueApi.Interfaces.Providers;
using DogQueueApi.Models;
using DogQueueApi.Models.Auth;
using DogQueueApi.Services;
using DogQueueApi.Validators;

namespace DogQueueApi.Managers
{
    public class AuthManager : IAuthManager
    {
        private readonly AppDbContext _context;
        private readonly ITokenProvider _tokenProvider;

        public AuthManager(AppDbContext context, ITokenProvider tokenProvider)
        {
            _context = context;
            _tokenProvider = tokenProvider;
        }

        public ServiceResult<object?> Register(User user)
        {
            var (isValid, errors) = UserValidator.ValidateRegister(user);
            if (!isValid)
            {
                return ServiceResult<object?>.BadRequest("Validation failed", errors);
            }

            if (_context.Users.Any(u => u.Username == user.Username))
            {
                return ServiceResult<object?>.BadRequest("Username already exists");
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return ServiceResult<object?>.Ok(new { message = "User registered successfully" });
        }

        public ServiceResult<LoginResponse> Login(LoginRequest login)
        {
            var (isValid, errors) = UserValidator.ValidateLogin(login);
            if (!isValid)
            {
                return ServiceResult<LoginResponse>.BadRequest("Validation failed", errors);
            }

            var user = _context.Users
                .FirstOrDefault(u => u.Username == login.Username && u.Password == login.Password);

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
}
