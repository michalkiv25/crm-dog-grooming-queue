using Microsoft.AspNetCore.Mvc;
using DogQueueApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DogQueueApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private static List<User> users = new List<User>();
        private static int nextId = 1;

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username) ||
                string.IsNullOrWhiteSpace(user.Password) ||
                string.IsNullOrWhiteSpace(user.FullName))
            {
                return BadRequest("All fields are required");
            }

            if (users.Any(u => u.Username == user.Username))
            {
                return BadRequest("Username already exists");
            }

            user.Id = nextId++;
            users.Add(user);

            Console.WriteLine("REGISTER USERS COUNT: " + users.Count);

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginUser)
        {
            var user = users.FirstOrDefault(u =>
                u.Username == loginUser.Username &&
                u.Password == loginUser.Password);

            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("THIS_IS_MY_SUPER_SECRET_KEY_12345");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "DogQueueApi",
                Audience = "DogQueueApi",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                message = "Login successful",
                token = tokenHandler.WriteToken(token),
                user
            });
        }
    }
}