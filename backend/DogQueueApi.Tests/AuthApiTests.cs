using System.Net;
using System.Net.Http.Json;
using DogQueueApi.Models;

namespace DogQueueApi.Tests;

public class AuthApiTests : IClassFixture<DogQueueApiApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(DogQueueApiApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_returns_ok_for_valid_payload()
    {
        var username = RandomLetterUsername();
        var user = new User
        {
            Username = username,
            Password = "secretpw",
            FullName = "Test User"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", user);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_returns_token_after_register()
    {
        var username = RandomLetterUsername();
        await _client.PostAsJsonAsync("/api/auth/register", new User
        {
            Username = username,
            Password = "secretpw",
            FullName = "Test User"
        });

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username,
            password = "secretpw"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var body = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(body?.Token);
        Assert.Equal(username, body.Username);
    }

    private static string RandomLetterUsername()
    {
        const string letters = "abcdefghijklmnopqrstuvwxyz";
        var rnd = Random.Shared;
        var chars = new char[12];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = letters[rnd.Next(letters.Length)];
        return new string(chars);
    }

    private sealed class LoginResponseDto
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
    }
}
