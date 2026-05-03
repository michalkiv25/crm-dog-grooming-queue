using DogQueueApi.Models;
using DogQueueApi.Validators;

namespace DogQueueApi.Tests;

public class UserValidatorTests
{
    [Fact]
    public void ValidateRegister_rejects_username_with_digits_when_only_letters_allowed()
    {
        var user = new User
        {
            Username = "user123",
            Password = "secretpw",
            FullName = "Test User"
        };

        var (ok, errors) = UserValidator.ValidateRegister(user);

        Assert.False(ok);
        Assert.Contains(errors, e => e.Contains("letters", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateRegister_accepts_letters_only_username_and_valid_password()
    {
        var user = new User
        {
            Username = "validuser",
            Password = "secretpw",
            FullName = "Test User"
        };

        var (ok, _) = UserValidator.ValidateRegister(user);

        Assert.True(ok);
    }
}
