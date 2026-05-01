using Microsoft.AspNetCore.Mvc;
using DogQueueApi.Interfaces.Managers;
using DogQueueApi.Models;
using DogQueueApi.Models.Auth;
using DogQueueApi.Services;

namespace DogQueueApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthManager _authManager;

        public AuthController(IAuthManager authManager)
        {
            _authManager = authManager;
        }

        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            var result = _authManager.Register(user);
            return ToActionResult(result);
        }

        // ---------------- LOGIN (JWT REAL) ----------------
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            var result = _authManager.Login(login);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult(ServiceResult<object?> result)
        {
            object payload = result.Errors?.Length > 0
                ? new { message = result.Message, errors = result.Errors }
                : result.Data ?? new { message = result.Message };

            return result.StatusCode switch
            {
                200 => Ok(payload),
                400 => BadRequest(payload),
                401 => Unauthorized(payload),
                403 => StatusCode(403, payload),
                404 => NotFound(payload),
                _ => StatusCode(result.StatusCode, payload)
            };
        }

        private IActionResult ToActionResult(ServiceResult<LoginResponse> result)
        {
            object payload = result.Errors?.Length > 0
                ? new { message = result.Message, errors = result.Errors }
                : (object?)result.Data ?? new { message = result.Message };

            return result.StatusCode switch
            {
                200 => Ok(payload),
                400 => BadRequest(payload),
                401 => Unauthorized(payload),
                403 => StatusCode(403, payload),
                404 => NotFound(payload),
                _ => StatusCode(result.StatusCode, payload)
            };
        }
    }
}