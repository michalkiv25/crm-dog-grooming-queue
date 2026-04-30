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

        // REGISTER
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
public IActionResult Login(LoginDto loginUser)
{
    var user = users.FirstOrDefault(u =>
        u.Username.Trim().ToLower() == loginUser.Username.Trim().ToLower() &&
        u.Password == loginUser.Password);

    if (user == null)
        return Unauthorized("Invalid username or password");

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim("FullName", user.FullName)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("THIS_IS_MY_SUPER_SECRET_KEY_12345"));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: "DogQueueApi",
        audience: "DogQueueApi",
        claims: claims,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: creds
    );

    return Ok(new
    {
        token = new JwtSecurityTokenHandler().WriteToken(token)
    });
}
}}