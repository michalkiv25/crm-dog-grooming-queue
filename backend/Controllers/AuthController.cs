using Microsoft.AspNetCore.Mvc;
using DogQueueApi.Data;
using DogQueueApi.Models;
using DogQueueApi.Validators;
using System.Linq;
using System;
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
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            // Validate user input
            var (isValid, errors) = UserValidator.ValidateRegister(user);
            if (!isValid)
            {
                return BadRequest(new { message = "Validation failed", errors });
            }

            if (_context.Users.Any(u => u.Username == user.Username))
            {
                return BadRequest(new { message = "Username already exists" });
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "User registered successfully" });
        }

        // ---------------- LOGIN (JWT REAL) ----------------
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            // Validate login input
            var (isValid, errors) = UserValidator.ValidateLogin(login);
            if (!isValid)
            {
                return BadRequest(new { message = "Validation failed", errors });
            }

            var user = _context.Users
                .FirstOrDefault(u => u.Username == login.Username && u.Password == login.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("THIS_IS_MY_SUPER_SECRET_KEY_12345"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "DogQueueApi",
                audience: "DogQueueApi",
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = jwt,
                username = user.Username,
                fullname = user.FullName
            });
        }
    }
}